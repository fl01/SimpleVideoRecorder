using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SimpleVideoRecorder.Core.ScreenCapture.Codec;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public class AviWriter : IDisposable, IAviStreamWriteHandler, IAviVideoWriter
    {
        private const int MAX_SUPER_INDEX_ENTRIES = 256;
        private const int MAX_INDEX_ENTRIES = 15000;
        private const int INDEX1_ENTRY_SIZE = 4 * sizeof(uint);
        private const int RIFF_AVI_SIZE_TRESHOLD = 512 * 1024 * 1024;
        private const int RIFF_AVIX_SIZE_TRESHOLD = int.MaxValue - 1024 * 1024;

        private readonly BinaryWriter fileWriter;
        private readonly object syncWrite = new object();
        private IList<IAviStreamInternal> streams = new List<IAviStreamInternal>();
        private bool isClosed = false;
        private bool startedWriting = false;
        private StreamInfo[] streamsInfo;
        private bool isFirstRiff = true;
        private RiffItem currentRiff;
        private RiffItem currentMovie;
        private RiffItem header;
        private int riffSizeTreshold;
        private int riffAviFrameCount = -1;
        private int index1Count = 0;
        private bool emitIndex1;
        private decimal framesPerSecond = 1;
        private uint frameRateNumerator;
        private uint frameRateDenominator;

        public string FileName { get; private set; }

        public decimal FramesPerSecond
        {
            get { return framesPerSecond; }
            set
            {
                Debug.Assert(value > 0);

                lock (syncWrite)
                {
                    CheckNotStartedWriting();
                    framesPerSecond = Decimal.Round(value, 3);
                }
            }
        }

        public bool EmitIndex1
        {
            get
            {
                return emitIndex1;
            }

            set
            {
                lock (syncWrite)
                {
                    CheckNotStartedWriting();
                    emitIndex1 = value;
                }
            }
        }

        public AviWriter(string fileName, int frameRate)
        {
            FileName = fileName;
            FramesPerSecond = frameRate;

            var fileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
            fileWriter = new BinaryWriter(fileStream);
        }

        void IAviStreamWriteHandler.WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            WriteStreamFrame(stream, isKeyFrame, frameData, startIndex, count);
        }

        void IAviStreamWriteHandler.WriteStreamFormat(AviVideoStream videoStream)
        {
            fileWriter.Write(40U); // size of structure
            fileWriter.Write(videoStream.Width);
            fileWriter.Write(videoStream.Height);
            fileWriter.Write((short)1); // planes
            fileWriter.Write((ushort)videoStream.BitsPerPixel); // bits per pixel
            fileWriter.Write((uint)videoStream.Codec); // compression (codec FOURCC)
            var sizeInBytes = videoStream.Width * videoStream.Height * (((int)videoStream.BitsPerPixel) / 8);
            fileWriter.Write((uint)sizeInBytes); // image size in bytes
            fileWriter.Write(0); // X pixels per meter
            fileWriter.Write(0); // Y pixels per meter

            // Writing grayscale palette for 8-bit uncompressed stream
            // Otherwise, no palette
            if (videoStream.BitsPerPixel == BitsPerPixel.Bpp8 && videoStream.Codec == KnownFourCC.Codecs.Uncompressed)
            {
                fileWriter.Write(256U); // palette colors used
                fileWriter.Write(0U); // palette colors important
                for (int i = 0; i < 256; i++)
                {
                    fileWriter.Write((byte)i);
                    fileWriter.Write((byte)i);
                    fileWriter.Write((byte)i);
                    fileWriter.Write((byte)0);
                }
            }
            else
            {
                fileWriter.Write(0U); // palette colors used
                fileWriter.Write(0U); // palette colors important
            }
        }

        void IAviStreamWriteHandler.WriteStreamHeader(AviVideoStream videoStream)
        {
            // See AVISTREAMHEADER structure
            fileWriter.Write((uint)videoStream.StreamType);
            fileWriter.Write((uint)videoStream.Codec);
            fileWriter.Write(0U); // StreamHeaderFlags
            fileWriter.Write((ushort)0); // priority
            fileWriter.Write((ushort)0); // language
            fileWriter.Write(0U); // initial frames
            fileWriter.Write(frameRateDenominator); // scale (frame rate denominator)
            fileWriter.Write(frameRateNumerator); // rate (frame rate numerator)
            fileWriter.Write(0U); // start
            fileWriter.Write((uint)streamsInfo[videoStream.Index].FrameCount); // length
            fileWriter.Write((uint)streamsInfo[videoStream.Index].MaxChunkDataSize); // suggested buffer size
            fileWriter.Write(0U); // quality
            fileWriter.Write(0U); // sample size
            fileWriter.Write((short)0); // rectangle left
            fileWriter.Write((short)0); // rectangle top
            short right = (short)(videoStream != null ? videoStream.Width : 0);
            short bottom = (short)(videoStream != null ? videoStream.Height : 0);
            fileWriter.Write(right); // rectangle right
            fileWriter.Write(bottom); // rectangle bottom
        }

        public void Close()
        {
            if (!isClosed)
            {
                bool finishWriting;
                lock (syncWrite)
                {
                    finishWriting = startedWriting;
                }
                // Call FinishWriting without holding the lock
                // because additional writes may be performed inside
                if (finishWriting)
                {
                    foreach (var stream in streams)
                    {
                        stream.FinishWriting();
                    }
                }

                lock (syncWrite)
                {
                    if (startedWriting)
                    {
                        foreach (var stream in streams)
                        {
                            FlushStreamIndex(stream);
                        }

                        CloseCurrentRiff();

                        // Rewrite header with actual data like frames count, super index, etc.
                        fileWriter.BaseStream.Position = header.ItemStart;
                        WriteHeader();
                    }

                    fileWriter.Close();
                    isClosed = true;
                }

                foreach (var disposableStream in streams.OfType<IDisposable>())
                {
                    disposableStream.Dispose();
                }
            }
        }

        public IAviVideoStream AddMotionJpegVideoStream(int width, int height, int quality)
        {
            var encoder = new MotionJpegVideoEncoderWpf(width, height, quality);

            return AddEncodingVideoStream(encoder, width, height, true);
        }

        public IAviVideoStream AddEncodingVideoStream(IVideoEncoder encoder, int width, int height, bool ownsEncoder)
        {
            return AddStream<IAviVideoStreamInternal>(index =>
            {
                var stream = new AviVideoStream(index, this, width, height, BitsPerPixel.Bpp32);
                var encodingStream = new EncodingVideoStreamWrapper(stream, encoder, ownsEncoder);
                var asyncStream = new AsyncVideoStreamWrapper(encodingStream);
                return asyncStream;
            });
        }

        private TStream AddStream<TStream>(Func<int, TStream> streamFactory)
            where TStream : IAviStreamInternal
        {
            lock (syncWrite)
            {
                CheckNotClosed();
                CheckNotStartedWriting();

                TStream stream = streamFactory.Invoke(streams.Count);
                streams.Add(stream);
                return stream;
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void FlushStreamIndex(IAviStreamInternal stream)
        {
            var si = streamsInfo[stream.Index];
            var index = si.StandardIndex;
            var entriesCount = index.Count;
            if (entriesCount == 0)
                return;

            var baseOffset = index[0].DataOffset;
            var indexSize = 24 + entriesCount * 8;

            CreateNewRiffIfNeeded(indexSize);

            // See AVISTDINDEX structure
            var chunk = fileWriter.OpenChunk(si.StandardIndexChunkId, indexSize);
            fileWriter.Write((ushort)2); // DWORDs per entry
            fileWriter.Write((byte)0); // index sub-type
            fileWriter.Write((byte)IndexType.Chunks); // index type
            fileWriter.Write((uint)entriesCount); // entries count
            fileWriter.Write((uint)stream.ChunkId); // chunk ID of the stream
            fileWriter.Write((ulong)baseOffset); // base offset for entries
            fileWriter.SkipBytes(sizeof(uint)); // reserved

            foreach (var entry in index)
            {
                fileWriter.Write((uint)(entry.DataOffset - baseOffset)); // chunk data offset
                fileWriter.Write(entry.DataSize); // chunk data size
            }

            fileWriter.CloseItem(chunk);

            var superIndex = streamsInfo[stream.Index].SuperIndex;
            var newEntry = new SuperIndexEntry
            {
                ChunkOffset = chunk.ItemStart,
                ChunkSize = chunk.ItemSize,
                Duration = entriesCount
            };
            superIndex.Add(newEntry);

            index.Clear();
        }

        private void CreateNewRiffIfNeeded(int approximateSizeOfNextChunk)
        {
            var estimatedSize = fileWriter.BaseStream.Position + approximateSizeOfNextChunk - currentRiff.ItemStart;
            if (isFirstRiff && EmitIndex1)
            {
                estimatedSize += RiffItem.ITEM_HEADER_SIZE + index1Count * INDEX1_ENTRY_SIZE;
            }
            if (estimatedSize > riffSizeTreshold)
            {
                CloseCurrentRiff();

                currentRiff = fileWriter.OpenList(KnownFourCC.Lists.AviExtended, KnownFourCC.ListTypes.Riff);
                currentMovie = fileWriter.OpenList(KnownFourCC.Lists.Movie);
            }
        }

        private void CheckNotStartedWriting()
        {
            if (startedWriting)
            {
                throw new InvalidOperationException("No stream information can be changed after starting to write frames.");
            }
        }

        private void CloseCurrentRiff()
        {
            fileWriter.CloseItem(currentMovie);

            // Several special actions for the first RIFF (AVI)
            if (isFirstRiff)
            {
                riffAviFrameCount = streams.OfType<IAviVideoStream>().Max(s => streamsInfo[s.Index].FrameCount);
                if (emitIndex1)
                {
                    WriteIndex1();
                }
                riffSizeTreshold = RIFF_AVIX_SIZE_TRESHOLD;
            }

            fileWriter.CloseItem(currentRiff);
            isFirstRiff = false;
        }

        private void WriteIndex1()
        {
            var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.Index1);

            var indices = streamsInfo.Select((si, i) => new { si.Index1, ChunkId = (uint)streams.ElementAt(i).ChunkId }).
                Where(a => a.Index1.Count > 0)
                .ToList();
            while (index1Count > 0)
            {
                var minOffset = indices[0].Index1[0].DataOffset;
                var minIndex = 0;
                for (var i = 1; i < indices.Count; i++)
                {
                    var offset = indices[i].Index1[0].DataOffset;
                    if (offset < minOffset)
                    {
                        minOffset = offset;
                        minIndex = i;
                    }
                }

                var index = indices[minIndex];
                fileWriter.Write(index.ChunkId);
                fileWriter.Write(index.Index1[0].IsKeyFrame ? 0x00000010U : 0);
                fileWriter.Write(index.Index1[0].DataOffset);
                fileWriter.Write(index.Index1[0].DataSize);

                index.Index1.RemoveAt(0);
                if (index.Index1.Count == 0)
                {
                    indices.RemoveAt(minIndex);
                }

                index1Count--;
            }

            fileWriter.CloseItem(chunk);
        }

        private void WriteHeader()
        {
            header = fileWriter.OpenList(KnownFourCC.Lists.Header);
            WriteFileHeader();
            foreach (var stream in streams)
            {
                WriteStreamList(stream);
            }
            WriteOdmlHeader();
            WriteJunkInsteadOfMissingSuperIndexEntries();
            fileWriter.CloseItem(header);
        }

        private void WriteStreamList(IAviStreamInternal stream)
        {
            var list = fileWriter.OpenList(KnownFourCC.Lists.Stream);
            WriteStreamHeader(stream);
            WriteStreamFormat(stream);
            WriteStreamName(stream);
            WriteStreamSuperIndex(stream);
            fileWriter.CloseItem(list);
        }

        private void WriteStreamHeader(IAviStreamInternal stream)
        {
            var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.StreamHeader);
            stream.WriteHeader();
            fileWriter.CloseItem(chunk);
        }

        private void WriteStreamFormat(IAviStreamInternal stream)
        {
            var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.StreamFormat);
            stream.WriteFormat();
            fileWriter.CloseItem(chunk);
        }

        private void WriteStreamName(IAviStream stream)
        {
            if (!string.IsNullOrEmpty(stream.Name))
            {
                var bytes = Encoding.ASCII.GetBytes(stream.Name);
                var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.StreamName);
                fileWriter.Write(bytes);
                fileWriter.Write((byte)0);
                fileWriter.CloseItem(chunk);
            }
        }

        private void WriteStreamSuperIndex(IAviStream stream)
        {
            var superIndex = streamsInfo[stream.Index].SuperIndex;

            // See AVISUPERINDEX structure
            var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.StreamIndex);
            fileWriter.Write((ushort)4); // DWORDs per entry
            fileWriter.Write((byte)0); // index sub-type
            fileWriter.Write((byte)IndexType.Indexes); // index type
            fileWriter.Write((uint)superIndex.Count); // entries count
            fileWriter.Write((uint)((IAviStreamInternal)stream).ChunkId); // chunk ID of the stream
            fileWriter.SkipBytes(3 * sizeof(uint)); // reserved

            // entries
            foreach (var entry in superIndex)
            {
                fileWriter.Write((ulong)entry.ChunkOffset); // offset of sub-index chunk
                fileWriter.Write((uint)entry.ChunkSize); // size of sub-index chunk
                fileWriter.Write((uint)entry.Duration); // duration of sub-index data (number of frames it refers to)
            }

            fileWriter.CloseItem(chunk);
        }

        private void WriteJunkInsteadOfMissingSuperIndexEntries()
        {
            var missingEntriesCount = streamsInfo.Sum(si => MAX_SUPER_INDEX_ENTRIES - si.SuperIndex.Count);
            if (missingEntriesCount > 0)
            {
                var junkDataSize = missingEntriesCount * sizeof(uint) * 4 - RiffItem.ITEM_HEADER_SIZE;
                var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.Junk, junkDataSize);
                fileWriter.SkipBytes(junkDataSize);
                fileWriter.CloseItem(chunk);
            }
        }

        private void WriteFileHeader()
        {
            // See AVIMAINHEADER structure
            var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.AviHeader);
            fileWriter.Write((uint)Decimal.Round(1000000m / FramesPerSecond)); // microseconds per frame
            // TODO: More correct computation of byterate
            fileWriter.Write((uint)Decimal.Truncate(FramesPerSecond * streamsInfo.Sum(s => s.MaxChunkDataSize))); // max bytes per second
            fileWriter.Write(0U); // padding granularity
            var flags = MainHeaderFlags.IsInterleaved | MainHeaderFlags.TrustChunkType;
            if (emitIndex1)
            {
                flags |= MainHeaderFlags.HasIndex;
            }
            fileWriter.Write((uint)flags); // MainHeaderFlags
            fileWriter.Write(riffAviFrameCount); // total frames (in the first RIFF list containing this header)
            fileWriter.Write(0U); // initial frames
            fileWriter.Write(streams.Count); // stream count
            fileWriter.Write(0U); // suggested buffer size
            var firstVideoStream = streams.OfType<IAviVideoStream>().First();
            fileWriter.Write(firstVideoStream.Width); // video width
            fileWriter.Write(firstVideoStream.Height); // video height
            fileWriter.SkipBytes(4 * sizeof(uint)); // reserved
            fileWriter.CloseItem(chunk);
        }

        private void WriteOdmlHeader()
        {
            var list = fileWriter.OpenList(KnownFourCC.Lists.OpenDml);
            var chunk = fileWriter.OpenChunk(KnownFourCC.Chunks.OpenDmlHeader);
            fileWriter.Write(streams.OfType<IAviVideoStream>().Max(s => streamsInfo[s.Index].FrameCount)); // total frames in file
            fileWriter.SkipBytes(61 * sizeof(uint)); // reserved
            fileWriter.CloseItem(chunk);
            fileWriter.CloseItem(list);
        }

        private void CheckNotClosed()
        {
            if (isClosed)
            {
                throw new ObjectDisposedException(typeof(AviWriter).Name);
            }
        }
        private void PrepareForWriting()
        {
            startedWriting = true;
            foreach (var stream in streams)
            {
                stream.PrepareForWriting();
            }
            AviUtils.SplitFrameRate(FramesPerSecond, out frameRateNumerator, out frameRateDenominator);

            streamsInfo = streams.Select(s => new StreamInfo(KnownFourCC.Chunks.IndexData(s.Index))).ToArray();

            riffSizeTreshold = RIFF_AVI_SIZE_TRESHOLD;

            currentRiff = fileWriter.OpenList(KnownFourCC.Lists.Avi, KnownFourCC.ListTypes.Riff);
            WriteHeader();
            currentMovie = fileWriter.OpenList(KnownFourCC.Lists.Movie);
        }

        private void WriteStreamFrame(AviStreamBase stream, bool isKeyFrame, byte[] frameData, int startIndex, int count)
        {
            lock (syncWrite)
            {
                CheckNotClosed();

                if (!startedWriting)
                {
                    PrepareForWriting();
                }

                var si = streamsInfo[stream.Index];
                if (si.SuperIndex.Count == MAX_SUPER_INDEX_ENTRIES)
                {
                    throw new InvalidOperationException("Cannot write more frames to this stream.");
                }

                if (ShouldFlushStreamIndex(si.StandardIndex))
                {
                    FlushStreamIndex(stream);
                }

                var shouldCreateIndex1Entry = emitIndex1 && isFirstRiff;

                CreateNewRiffIfNeeded(count + (shouldCreateIndex1Entry ? INDEX1_ENTRY_SIZE : 0));

                var chunk = fileWriter.OpenChunk(stream.ChunkId, count);
                fileWriter.Write(frameData, startIndex, count);
                fileWriter.CloseItem(chunk);

                si.OnFrameWritten(chunk.DataSize);
                var dataSize = (uint)chunk.DataSize;
                // Set highest bit for non-key frames according to the OpenDML spec
                if (!isKeyFrame)
                {
                    dataSize |= 0x80000000U;
                }

                var newEntry = new StandardIndexEntry
                {
                    DataOffset = chunk.DataStart,
                    DataSize = dataSize
                };
                si.StandardIndex.Add(newEntry);

                if (shouldCreateIndex1Entry)
                {
                    var index1Entry = new Index1Entry
                    {
                        IsKeyFrame = isKeyFrame,
                        DataOffset = (uint)(chunk.ItemStart - currentMovie.DataStart),
                        DataSize = dataSize
                    };
                    si.Index1.Add(index1Entry);
                    index1Count++;
                }
            }
        }

        private bool ShouldFlushStreamIndex(IList<StandardIndexEntry> index)
        {
            return index.Count >= MAX_INDEX_ENTRIES || index.Count > 0 && fileWriter.BaseStream.Position - index[0].DataOffset > uint.MaxValue;
        }
    }
}

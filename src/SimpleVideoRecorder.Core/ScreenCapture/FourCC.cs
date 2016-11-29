namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public class FourCC
    {
        private readonly string fcc;
        private readonly uint fccDword;

        public string Code
        {
            get { return fcc; }
        }

        public FourCC(uint value)
        {
            fccDword = value;
            fcc = new string
                      (
                        new[]
                        {
                            (char)(value & 0xFF),
                            (char)((value & 0xFF00) >> 8),
                            (char)((value & 0xFF0000) >> 16),
                            (char)((value & 0xFF000000U) >> 24)
                        }
                      );
        }

        public FourCC(string fourCharacterCode)
        {
            fcc = fourCharacterCode.PadRight(4);

            for (int i = 0; i < fcc.Length; i++)
            {
                fccDword += (uint)fcc[i] << i * 8;
            }
        }

        public static implicit operator FourCC(uint value)
        {
            return new FourCC(value);
        }

        public static implicit operator FourCC(string value)
        {
            return new FourCC(value);
        }

        public static explicit operator uint(FourCC value)
        {
            return value.fccDword;
        }

        public static explicit operator string(FourCC value)
        {
            return value.fcc;
        }

        public static bool operator ==(FourCC value1, FourCC value2)
        {
            return value1.fccDword == value2.fccDword;
        }

        public static bool operator !=(FourCC value1, FourCC value2)
        {
            return !(value1 == value2);
        }

        public override int GetHashCode()
        {
            return fccDword.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as FourCC;
            return other != null && other == this;
        }
    }
}

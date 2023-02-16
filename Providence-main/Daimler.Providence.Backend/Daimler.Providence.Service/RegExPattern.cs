namespace Daimler.Providence.Service
{
    public static class RegExPattern
    {
        public const string AlphaNumericWithSpaceAnd = "^[a-zA-Z0-9& ]*$";

        public const string AlphaNumericWithSpaceComma = "^[a-zA-Z0-9, ]*$";

        public const string AlphaNumericWithSomespecialcharacters = "^[^>#}<{]*$";
    }
}

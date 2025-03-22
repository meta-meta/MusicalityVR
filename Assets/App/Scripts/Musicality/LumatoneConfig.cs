namespace Musicality
{
    public class LumatoneConfig
    {
        public static int[] RowLengths = new[]
        {
            2,
            5,
            8,
            11,
            14,
            17,
            20,
            23,
            26,
            28,
            26,
            23,
            20,
            17,
            14,
            11,
            8,
            5,
            2,
        };
       
        public static int[] RowStarts = new[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            1,
            4,
            7,
            10,
            13,
            16,
            19,
            22,
            25,
            28,
        };
       
        public static int[] BoardRowLengths = new[]
        {
            2,
            5,
            6, 6, 6, 6, 6, 6, 6,
            5,
            2,
        };
    }
}
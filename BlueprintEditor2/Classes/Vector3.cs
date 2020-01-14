namespace BlueprintEditor2
{
    public class Vector3
    {
        public static readonly Vector3 Zero = new Vector3(0,0,0);

        public int X;
        public int Y;
        public int Z;

        public Vector3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
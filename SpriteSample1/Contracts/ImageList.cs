namespace SpriteSample1.Contracts
{
    // class for the ImageList1
    public class ImageList
    {
        public string Id { get; set; }

        public string FileName { get; set; }

        public Location Coordinate { get; set; }

        public Dimension Dimension { get; set; }

        public string Identifier { get; set; }
    }

    public class Location
    {
        public int X { get; set; }

        public int Y { get; set; }
    }

    public class Dimension
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }
}

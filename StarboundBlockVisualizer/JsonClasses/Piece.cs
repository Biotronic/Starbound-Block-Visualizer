namespace StarboundBlockVisualizer.JsonClasses
{
    public class Piece
    {
        public string Texture { get; set; }
        public Point TextureSize { get; set; }
        public Point TexturePosition { get; set; }
        public Point ColorStride { get; set; }
        public Point VariantStride { get; set; }
    }
}
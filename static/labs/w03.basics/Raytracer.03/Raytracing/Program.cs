namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Scene world = new Scene();
        world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f)); 
        world.Add(new Plane(new Vector3(0, -0.5f, 0), new Vector3(0, 1, 0)));

        Camera cam = new Camera();
        cam.Position = new Vector3(0, 0, 1);
        cam.Target = new Vector3(0, 0, -1);

        Image image = cam.Render(world);

        Console.Write(image.ToString());
    }
}
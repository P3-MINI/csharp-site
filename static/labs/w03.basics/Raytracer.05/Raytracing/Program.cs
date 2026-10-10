namespace Raytracing;

class Program
{
    static void Main(string[] args)
    {
        Scene world = new Scene();
        // Add a sky sphere
        world.Add(new Sphere(new Vector3(0, 0, 0), 1000.0f, new EmissiveMaterial(new Vector3(0.5f, 0.7f, 1.0f))));

        // Add 3 spheres
        world.Add(new Sphere(new Vector3(0, 0, -1), 0.5f, new Lambertian(new Vector3(1, 0, 0)))); 
        world.Add(new Sphere(new Vector3(-1.1f, 0, -1), 0.5f, new Lambertian(new Vector3(0, 0, 1)))); 
        world.Add(new Sphere(new Vector3(1.1f, 0, -1), 0.5f, new Lambertian(new Vector3(1, 1, 0)))); 
        
        // Add a green ground plane
        world.Add(new Plane(new Vector3(0, -0.5f, 0), new Vector3(0, 1, 0), new Lambertian(new Vector3(0.2f, 0.8f, 0.2f))));

        Camera cam = new Camera();
        cam.Position = new Vector3(0, 0, 1);
        cam.Target = new Vector3(0, 0, -1);

        Image image = cam.Render(world);

        Console.Write(image.ToString());
    }
}
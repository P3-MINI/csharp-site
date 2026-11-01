namespace Raytracing;

public class Camera
{
    private float _aspectRatio = 16.0f / 9.0f;
    private int _imageWidth = 1280;
    private Vector3 _position = new Vector3(0, 0, 0);
    private Vector3 _target = new Vector3(0, 0, -1);
    private float _fov = 90.0f;

    public float AspectRatio 
    { 
        get => _aspectRatio; 
        set { _aspectRatio = value; Recalculate(); } 
    }
    
    public int ImageWidth 
    { 
        get => _imageWidth; 
        set { _imageWidth = value; Recalculate(); } 
    }

    public int ImageHeight => _imageHeight;

    public Vector3 Position 
    { 
        get => _position; 
        set { _position = value; Recalculate(); } 
    }

    public Vector3 Target 
    { 
        get => _target; 
        set { _target = value; Recalculate(); } 
    }

    public float Fov 
    { 
        get => _fov; 
        set { _fov = value; Recalculate(); } 
    }

    private int _imageHeight;
    private Vector3 _viewportCorner;
    private Vector3 _pixelDu;
    private Vector3 _pixelDv;

    public Camera()
    {
        Recalculate();
    }

    private void Recalculate()
    {
        _imageHeight = (int)(_imageWidth / _aspectRatio);
        if (_imageHeight < 1) 
        {
            _imageHeight = 1;
        }

        float theta = _fov * (MathF.PI / 180.0f);
        float h = MathF.Tan(theta / 2.0f);
        float distanceToViewport = (Position - Target).Length();
        float viewportHeight = 2.0f * h * distanceToViewport;
        float viewportWidth = viewportHeight * AspectRatio;

        Vector3 w = (Position - Target).Normalize();
        Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), w).Normalize();
        Vector3 v = Vector3.Cross(w, u);

        Vector3 viewportU = viewportWidth * u;
        Vector3 viewportV = viewportHeight * -v;

        _pixelDu = viewportU / _imageWidth;
        _pixelDv = viewportV / _imageHeight;

        Vector3 viewportUpperLeft = Position - (distanceToViewport * w) - viewportU / 2.0f - viewportV / 2.0f;
        _viewportCorner = viewportUpperLeft + 0.5f * (_pixelDu + _pixelDv);
    }

    public int MaxDepth { get; set; } = 50;
    public int SamplesPerPixel { get; set; } = 100;

    public Image Render(IHittable world)
    {
        Image image = new Image(_imageWidth, _imageHeight);

        for (int j = 0; j < _imageHeight; j++)
        {
            for (int i = 0; i < _imageWidth; i++)
            {
                Vector3 pixelColor = new Vector3(0, 0, 0);

                for (int sample = 0; sample < SamplesPerPixel; sample++)
                {
                    float offsetX = (float)System.Random.Shared.NextDouble() - 0.5f;
                    float offsetY = (float)System.Random.Shared.NextDouble() - 0.5f;

                    Vector3 pixelSample = _viewportCorner + ((i + offsetX) * _pixelDu) + ((j + offsetY) * _pixelDv);
                    Vector3 rayDirection = pixelSample - Position;
                    Ray ray = new Ray(Position, rayDirection);

                    pixelColor += RayColor(ray, MaxDepth, world);
                }

                image[i, j] = pixelColor / SamplesPerPixel;
            }
        }

        return image;
    }

    private Vector3 RayColor(Ray ray, int depth, IHittable world)
    {
        if (depth <= 0)
        {
            return new Vector3(0, 0, 0);
        }

        if (world.Hit(ray, 0.001f, float.PositiveInfinity, out HitInfo hitInfo))
        {
            Vector3 emitted = hitInfo.Mat?.Emit() ?? new Vector3(0, 0, 0);

            if (hitInfo.Mat != null && hitInfo.Mat.Scatter(in ray, ref hitInfo, out Vector3 attenuation, out Ray scattered))
            {
                return emitted + attenuation * RayColor(scattered, depth - 1, world);
            }
            return emitted;
        }

        // no hit = black
        return new Vector3(0, 0, 0); 
    }
}

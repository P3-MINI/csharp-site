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

    private int _height;
    private Vector3 _viewportCorner;
    private Vector3 _pixelDu;
    private Vector3 _pixelDv;

    public Camera()
    {
        Recalculate();
    }

    private void Recalculate()
    {
        _height = (int)(_imageWidth / _aspectRatio);
        if (_height < 1) 
        {
            _height = 1;
        }

        float theta = _fov * (MathF.PI / 180.0f);
        float h = MathF.Tan(theta / 2.0f);
        float distanceToViewport = (Position - Target).Length();
        float viewportHeight = 2.0f * h * distanceToViewport;
        float viewportWidth = viewportHeight * ((float)_imageWidth / _height);

        Vector3 w = (Position - Target).Normalize();
        Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), w).Normalize();
        Vector3 v = Vector3.Cross(w, u);

        Vector3 viewportU = viewportWidth * u;
        Vector3 viewportV = viewportHeight * -v;

        _pixelDu = viewportU / _imageWidth;
        _pixelDv = viewportV / _height;

        Vector3 viewportUpperLeft = Position - (distanceToViewport * w) - viewportU / 2.0f - viewportV / 2.0f;
        _viewportCorner = viewportUpperLeft + 0.5f * (_pixelDu + _pixelDv);
    }

    public Image Render(IHittable world)
    {
        Image image = new Image(_imageWidth, _height);

        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _imageWidth; i++)
            {
                Vector3 pixelCenter = _viewportCorner + (i * _pixelDu) + (j * _pixelDv);
                Vector3 rayDirection = pixelCenter - Position;
                Ray ray = new Ray(Position, rayDirection);

                if (world.Hit(ray, 0.001f, float.PositiveInfinity, out HitInfo hitInfo))
                {
                    image[i, j] = new Vector3(1, 0, 0); // hit = red
                }
                else
                {
                    image[i, j] = new Vector3(0, 0, 0); // no hit = black
                }
            }
        }

        return image;
    }
}

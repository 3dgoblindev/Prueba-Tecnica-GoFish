using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InfinitePan : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollSpeedX = 0.02f;
    public float scrollSpeedY = 0f;

    [Header("Tiling")]
    public Vector2 tiling = new Vector2(2f, 1f);

    private Material _mat;
    private Vector2 _offset;

    void Start()
    {
        // Instancia el material para no modificar el asset original
        _mat = GetComponent<Renderer>().material;
        _mat.mainTextureScale = tiling;
    }

    void Update()
    {
        _offset.x += scrollSpeedX * Time.deltaTime;
        _offset.y += scrollSpeedY * Time.deltaTime;

        // Mantenemos el valor en [0,1] para evitar float overflow
        _offset.x = Mathf.Repeat(_offset.x, 1f);
        _offset.y = Mathf.Repeat(_offset.y, 1f);

        _mat.mainTextureOffset = _offset;
    }

    void OnDestroy()
    {
        // Destruye la instancia del material al salir
        if (_mat != null)
            Destroy(_mat);
    }
}
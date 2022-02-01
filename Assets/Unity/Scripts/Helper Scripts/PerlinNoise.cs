using UnityEngine;
using System.Collections;

public class PerlinNoise : MonoBehaviour
{
    public bool move = false;
    public float changeSpeed = 0.5f;

    // Width and height of the texture in pixels.
    public int pixWidth;
    public int pixHeight;

    // The origin of the sampled area in the plane.
    public float xOrg;
    public float yOrg;

    public float directionX;
    public float directionY;
    public float scaleDeriv;

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 1.0F;

    private Texture2D noiseTex;
    private Color[] pix;
    private Renderer rend;

    void Start()
    {
        xOrg = Random.Range(20, 50);
        yOrg = Random.Range(20, 50);
        rend = GetComponent<Renderer>();
        directionX = Random.insideUnitSphere.x * Time.deltaTime * changeSpeed;
        directionY = Random.insideUnitCircle.y * Time.deltaTime;
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        pix = new Color[noiseTex.width * noiseTex.height];
        rend.material.mainTexture = noiseTex;
        InvokeRepeating(nameof(ChangeDirection), 0, 1/60f);

    }

    void CalcNoise()
    {
        // For each pixel in the texture...
        float y = 0.0F;

        while (y < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {
                float xCoord = xOrg + x / noiseTex.width * scale;
                float yCoord = yOrg + y / noiseTex.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                sample = (sample - 0.2f) / 0.8f;
                pix[(int)y * noiseTex.width + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }

    void Update()
    {
        CalcNoise();

        if (move)
        {
            xOrg += directionX;
            yOrg += directionY;
            scale = Mathf.Clamp(scale + scaleDeriv, 0f, 3f);
        }
        
    }

    void ChangeDirection()
    {
        directionX = Mathf.Clamp(directionX + (Random.insideUnitCircle.x * Time.deltaTime * changeSpeed), -0.01f, 0.01f);
        directionY = Mathf.Clamp(directionY + (Random.insideUnitCircle.y * Time.deltaTime * changeSpeed), -0.01f, 0.01f);
        scaleDeriv = Random.insideUnitCircle.x * Time.deltaTime * (changeSpeed * 3f);
    }
}
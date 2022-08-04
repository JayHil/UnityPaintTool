using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class painter : MonoBehaviour
{
    private Camera cam;
    private Vector3[] vectors;
    private int n;
    private float range;
    private float angle;
    private float scrollSpeed;
    private List<Renderer> edtMaterials;
    private List<Texture2D> origTextures;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        edtMaterials = new List<Renderer>();
        origTextures = new List<Texture2D>();
        n = 100;
        range = 10f;
        angle = 20f;
        scrollSpeed = 50f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) {
            StartCoroutine(paintSpray(transform.position, cam.transform.forward));
        }
        angle += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        angle = Mathf.Clamp(angle, 5f, 90f);
    }

    void OnApplicationQuit() {
        //for each edited texture, restore original texture
        for(int i = 0; i < edtMaterials.Count; i++) {
            edtMaterials[i].material.SetTexture("_MainTex", origTextures[i]);
        }
    }

    private IEnumerator paintSpray(Vector3 pos, Vector3 dir) {
        for (int i = 0; i < n; i++) {
            RaycastHit hit;
            float xRot = Random.Range(-angle, angle);
            float yRot = Random.Range(-angle, angle);
            float zRot = Random.Range(-angle, angle);

            if (!Physics.Raycast(pos, Quaternion.Euler(xRot, yRot, zRot) * dir, out hit, range, 1 << LayerMask.NameToLayer("Paintable"))) yield break;

            Renderer rend = hit.transform.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            rend.enabled = true;

            if (rend == null || rend.material == null || meshCollider == null) yield break;
            Texture2D tex;
            Material mat = rend.material;
            int xDim = 100;
            int yDim = 100;

            if (mat.GetTexture("_MainTex") == null) {
                tex = new Texture2D(xDim, yDim);
                Color[] tmpPixels = tex.GetPixels();
                for (int j = 0; j < tmpPixels.Length; j++) {
                    tmpPixels[j] *= new Color(0,0,0,0);
                }
                tex.SetPixels(tmpPixels);
                mat.SetTexture("_MainTex", tex);
            }

            origTextures.Add(mat.GetTexture("_MainTex") as Texture2D);
            edtMaterials.Add(rend);

            tex = mat.GetTexture("_MainTex") as Texture2D;
            tex.filterMode = FilterMode.Point;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.red);
            tex.Apply();
            yield break;
        }
    }
}

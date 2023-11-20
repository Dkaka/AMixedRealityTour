using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentObjectTransparent : MonoBehaviour
{
    public float alphaValue = 0.5f;
    private void Start()
    {

        StartCoroutine(DelayedFunction());
        // Get all the child renderers
        //Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        //GameObject [] childRenderers = GetComponentsInChildren<Renderer>();

        
    }
    IEnumerator DelayedFunction()
    {
        yield return new WaitForSeconds(10f);

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Iterate through the mesh renderers and modify their materials
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Material[] materials = meshRenderer.sharedMaterials;

            // Modify the transparency of each material
            foreach (Material material in materials)
            {
                if (material != null)
                {
                    //Color color = material.color;
                    //color.a = 0.5f/* set your desired transparency value between 0 and 1 */;
                    //material.color = color;
                    int alphaPropertyID = Shader.PropertyToID("_Alpha");

                    // Set the new alpha value
                    material.SetFloat(alphaPropertyID, alphaValue);
                }
                else
                {
                    // The GameObject was not found
                    Debug.Log("matrial null");
                }
            }
        }

        //// Loop through each child renderer
        //foreach (Renderer renderer in childRenderers)
        //{
        //    // Get the materials of the child renderer
        //    Material[] materials = renderer.materials;

        //    // Loop through each material and modify its rendering properties
        //    for (int i = 0; i < materials.Length; i++)
        //    {
        //        Color newColor = materials[i].color;  // Get the current color

        //        newColor.a = 0.5f;  // Modify the alpha component (transparency)

        //        materials[i].color = newColor;  // Assign the modified color back to the property
        //        //materials[i].color.a = 0.5f;
        //        // Set the material's rendering mode to transparent
        //        //materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //        //materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //        //materials[i].SetInt("_ZWrite", 0);
        //        //materials[i].DisableKeyword("_ALPHATEST_ON");
        //        //materials[i].EnableKeyword("_ALPHABLEND_ON");
        //        //materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //        //materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        //    }
        //}
    }
}
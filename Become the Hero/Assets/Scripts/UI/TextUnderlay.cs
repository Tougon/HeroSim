
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TMPro.Examples
{
    // Example script to simulate spell checking
    public class TextUnderlay : MonoBehaviour
    {
        public Texture HightlightTexture;
        public float HightlightSize = 5;
        public float HightlightHeight = 5;

        private TMP_Text m_TextComponent;

        private GameObject m_WordHighlighter;

        private Mesh m_Mesh;
        private TMP_MeshInfo m_MeshInfo;

        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;

        private CanvasRenderer m_CanvasRenderer;

        private Material m_WavyLineMaterial;

        // Static text with array containing misspelled words.
        private string m_SourceText = "This is an examle script to demonstrte how words could be highlightd to show misspeled words in a text object.";


        void Awake()
        {
            m_TextComponent = GetComponent<TMP_Text>();
            m_TextComponent.text = m_SourceText;


            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                m_MeshInfo = new TMP_MeshInfo(m_Mesh, 8);
            }


            // Create object that will hold the geometry used to highlight misspelled words.
            if (m_WordHighlighter == null)
            {
                m_WordHighlighter = new GameObject();
                m_WordHighlighter.transform.SetParent(this.transform, false);

                // Create new material which supports tiling and assign the wavy line texture to it.
                m_WavyLineMaterial = new Material(Shader.Find("TextMeshPro/Sprite"));
                m_WavyLineMaterial.SetTexture(ShaderUtilities.ID_MainTex, HightlightTexture);

                // Determine the type of TMP component
                if (m_TextComponent as TextMeshPro != null)
                {
                    // Add the required components for the object type.
                    m_MeshRenderer = m_WordHighlighter.AddComponent<MeshRenderer>();
                    m_MeshRenderer.sharedMaterial = m_WavyLineMaterial;

                    m_MeshFilter = m_WordHighlighter.AddComponent<MeshFilter>();
                    m_MeshFilter.mesh = m_Mesh;
                }
                else if (m_TextComponent as TextMeshProUGUI != null)
                {
                    m_CanvasRenderer = m_WordHighlighter.AddComponent<CanvasRenderer>();
                    m_CanvasRenderer.SetMaterial(m_WavyLineMaterial, HightlightTexture);
                }
            }
        }


        void OnEnable()
        {
            // Subscribe to event fired when text object has been regenerated.
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }


        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        void OnDestroy()
        {
            if (m_Mesh != null)
                DestroyImmediate(m_Mesh);
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_MeshRenderer != null && m_WavyLineMaterial != null && HightlightTexture != null)
            {
                m_WavyLineMaterial.SetTexture(ShaderUtilities.ID_MainTex, HightlightTexture);
                ON_TEXT_CHANGED(m_TextComponent);
            }

            if (m_CanvasRenderer != null && m_WavyLineMaterial != null && HightlightTexture != null)
            {
                m_CanvasRenderer.SetMaterial(m_WavyLineMaterial, HightlightTexture);
                ON_TEXT_CHANGED(m_TextComponent);
            }
        }
#endif


        /// <summary>
        /// Event called when a text object has been updated.
        /// </summary>
        /// <param name="obj">Object.</param>
        void ON_TEXT_CHANGED(Object obj)
        {
            if (obj != m_TextComponent || m_WordHighlighter == null)
                return;

            int wordCount = m_TextComponent.textInfo.wordCount;

            TMP_TextInfo textInfo = m_TextComponent.textInfo;

            // Add function to check source text to find misspelled words.
            // For this example, the input text is static and I am using an array to indicate which words
            // are misspelled.

            int numberOfMisspelledWords = textInfo.wordInfo.Length;

            // Make sure our Mesh allocations can hold the required geometry.
            if (m_MeshInfo.vertices == null)
                m_MeshInfo = new TMP_MeshInfo(m_Mesh, numberOfMisspelledWords + 1);
            else if (m_MeshInfo.vertices.Length < numberOfMisspelledWords * 4)
                m_MeshInfo.ResizeMeshInfo(Mathf.NextPowerOfTwo(numberOfMisspelledWords + 1));

            // Clear current geometry
            m_MeshInfo.Clear(true);

            for (int i = 0; i < textInfo.wordInfo.Length; i++)
            {
                int first = textInfo.wordInfo[i].firstCharacterIndex;
                int last = textInfo.wordInfo[i].lastCharacterIndex;
                float wordScale = textInfo.characterInfo[first].scale;

                // Define a quad from first to last character of the word.
                Vector3 bl = new Vector3(textInfo.characterInfo[first].bottomLeft.x, textInfo.characterInfo[first].baseLine - HightlightHeight * wordScale, 0);
                Vector3 tl = new Vector3(bl.x, textInfo.characterInfo[first].baseLine, 0);
                Vector3 tr = new Vector3(textInfo.characterInfo[last].topRight.x, tl.y, 0);
                Vector3 br = new Vector3(tr.x, bl.y, 0);

                int index_X4 = i * 4;

                Vector3[] vertices = m_MeshInfo.vertices;
                vertices[index_X4 + 0] = bl;
                vertices[index_X4 + 1] = tl;
                vertices[index_X4 + 2] = tr;
                vertices[index_X4 + 3] = br;

                Vector2[] uvs0 = m_MeshInfo.uvs0;
                float length = Mathf.Abs(tr.x - bl.x) / wordScale * HightlightSize;
                float tiling = length / (HightlightTexture == null ? 1 : HightlightTexture.width);

                uvs0[index_X4 + 0] = new Vector2(0, 0);
                uvs0[index_X4 + 1] = new Vector2(0, 1);
                uvs0[index_X4 + 2] = new Vector2(tiling, 1);
                uvs0[index_X4 + 3] = new Vector2(tiling, 0);
            }

            // Push changes into meshes
            m_Mesh.vertices = m_MeshInfo.vertices;
            m_Mesh.uv = m_MeshInfo.uvs0;
            m_Mesh.RecalculateBounds();

            // The canvas system requires using SetMesh whenever changes to the geometry are made.
            if (m_TextComponent as TextMeshProUGUI != null)
                m_CanvasRenderer.SetMesh(m_Mesh);

        }
    }
}
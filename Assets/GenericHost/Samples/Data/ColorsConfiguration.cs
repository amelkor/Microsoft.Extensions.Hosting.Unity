using UnityEngine;

namespace GenericHost.Samples
{
    [CreateAssetMenu(fileName = "ColorsConfiguration", menuName = "Generic Host Sample/Colors SO", order = 0)]
    public class ColorsConfiguration : ScriptableObject
    {
        [SerializeField] private Color[] colors;

        public Color GetRandomColor() => colors[Random.Range(0, colors.Length - 1)];
    }
}
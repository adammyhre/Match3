using UnityEngine;

namespace Match3 {
    [RequireComponent(typeof(SpriteRenderer))]
    public class Gem : MonoBehaviour {
        public GemType type;

        public void SetType(GemType type) {
            this.type = type;
            GetComponent<SpriteRenderer>().sprite = type.sprite;
        }
        
        public GemType GetType() => type;
    }
}
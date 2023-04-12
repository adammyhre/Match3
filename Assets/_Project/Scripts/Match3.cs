using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Match3 {
    public class Match3 : MonoBehaviour {
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;
        
        [SerializeField] Gem gemPrefab;
        [SerializeField] GemType[] gemTypes;
        [SerializeField] Ease ease = Ease.InQuad;

        GridSystem2D<GridObject<Gem>> grid;

        InputReader inputReader;
        Vector2Int selectedGem = Vector2Int.one * -1;

        void Awake() {
            inputReader = GetComponent<InputReader>();
        }
        
        void Start() {
            InitializeGrid();
            inputReader.Fire += OnSelectGem;
        }

        void OnDestroy() {
            inputReader.Fire -= OnSelectGem;
        }

        void OnSelectGem() {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));
            
            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;
            
            if (selectedGem == gridPos) {
                DeselectGem();
            } else if (selectedGem == Vector2Int.one * -1) {
                SelectGem(gridPos);
            } else {
                StartCoroutine(RunGameLoop(selectedGem, gridPos));
            }
        }

        IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB) {
            yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            
            // Matches?
            // Make Gems explode
            // Fill empty spots

            DeselectGem();
        }

        IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB) {
            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);
            
            // See README for a link to the DOTween asset
            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);
            
            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);
            
            yield return new WaitForSeconds(0.5f);
        }

        void InitializeGrid() {
            grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, debug);
            
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    CreateGem(x, y);
                }
            }
        }

        void CreateGem(int x, int y) {
            var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
            gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
            var gridObject = new GridObject<Gem>(grid, x, y);
            gridObject.SetValue(gem);
            grid.SetValue(x, y, gridObject);
        }

        void DeselectGem() => selectedGem = new Vector2Int(-1, -1);
        void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;

        bool IsValidPosition(Vector2 gridPosition) {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
    }
}
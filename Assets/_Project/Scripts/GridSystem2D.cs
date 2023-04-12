using System;
using TMPro;
using UnityEngine;

namespace Match3 {
    /// <summary>
    /// A generic 2D grid implementation that can be used in both 2D and 3D.
    /// The grid stores values of a specified type and provides methods for accessing and modifying
    /// these values based on grid coordinates or world coordinates. The grid can be displayed
    /// either vertically (X-Y plane) or horizontally (X-Z plane) using a CoordinateConverter.
    /// </summary>
    public class GridSystem2D<T> {
        readonly int width;
        readonly int height;
        readonly float cellSize;
        readonly Vector3 origin;
        readonly T[,] gridArray;

        readonly CoordinateConverter coordinateConverter;

        public event Action<int, int, T> OnValueChangeEvent;
        
        public static GridSystem2D<T> VerticalGrid(int width, int height, float cellSize, Vector3 origin, bool debug = false) {
            return new GridSystem2D<T>(width, height, cellSize, origin, new VerticalConverter(), debug);
        }
        
        public static GridSystem2D<T> HorizontalGrid(int width, int height, float cellSize, Vector3 origin, bool debug = false) {
            return new GridSystem2D<T>(width, height, cellSize, origin, new HorizontalConverter(), debug);
        }

        public GridSystem2D(int width, int height, float cellSize, Vector3 origin, CoordinateConverter coordinateConverter, bool debug) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.origin = origin;
            this.coordinateConverter = coordinateConverter ?? new VerticalConverter();

            gridArray = new T[width, height];
            
            if (debug) {
                DrawDebugLines();
            }
        }
        
        // Set a value from a grid position
        public void SetValue(Vector3 worldPosition, T value) {
            Vector2Int pos = coordinateConverter.WorldToGrid(worldPosition, cellSize, origin);
            SetValue(pos.x, pos.y, value);
        }

        public void SetValue(int x, int y, T value) {
            if (IsValid(x, y)) {
                gridArray[x, y] = value;
                OnValueChangeEvent?.Invoke(x, y, value);
            }
        }
        
        // Get a value from a grid position
        public T GetValue(Vector3 worldPosition) {
            Vector2Int pos = GetXY(worldPosition);
            return GetValue(pos.x, pos.y);
        }

        public T GetValue(int x, int y) {
            return IsValid(x, y) ? gridArray[x, y] : default;
        }

        bool IsValid(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;
        
        public Vector2Int GetXY(Vector3 worldPosition) => coordinateConverter.WorldToGrid(worldPosition, cellSize, origin);
        
        public Vector3 GetWorldPositionCenter(int x, int y) => coordinateConverter.GridToWorldCenter(x, y, cellSize, origin);

        Vector3 GetWorldPosition(int x, int y) => coordinateConverter.GridToWorld(x, y, cellSize, origin);
        
        void DrawDebugLines() {
            const float duration = 100f;
            var parent = new GameObject("Debugging");

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    CreateWorldText(parent, x + "," + y, GetWorldPositionCenter(x, y), coordinateConverter.Forward);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, duration);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, duration);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, duration);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, duration);
        }
        
        TextMeshPro CreateWorldText(GameObject parent, string text, Vector3 position, Vector3 dir, 
            int fontSize = 2, Color color = default, TextAlignmentOptions textAnchor = TextAlignmentOptions.Center, int sortingOrder = 0) 
        {
            GameObject gameObject = new GameObject("DebugText_" + text, typeof(TextMeshPro));
            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.position = position;
            gameObject.transform.forward = dir;

            TextMeshPro textMeshPro = gameObject.GetComponent<TextMeshPro>();
            textMeshPro.text = text;
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = color == default ? Color.white : color;
            textMeshPro.alignment = textAnchor;
            textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMeshPro;
        }

        public abstract class CoordinateConverter {
            public abstract Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin);
            
            public abstract Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin);

            public abstract Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin);

            public abstract Vector3 Forward { get; }
        }
        
        /// <summary>
        /// A coordinate converter for vertical grids, where the grid lies on the X-Y plane.
        /// </summary>
        public class VerticalConverter : CoordinateConverter {
            public override Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin) {
                return new Vector3(x, y, 0) * cellSize + origin;
            }
            
            public override Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin) {
                return new Vector3(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f, 0) + origin;
            }

            public override Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin) {
                Vector3 gridPosition = (worldPosition - origin) / cellSize;
                var x = Mathf.FloorToInt(gridPosition.x);
                var y = Mathf.FloorToInt(gridPosition.y);
                return new Vector2Int(x, y);
            }
        
            public override Vector3 Forward => Vector3.forward;
        }
        
        /// <summary>
        /// A coordinate converter for horizontal grids, where the grid lies on the X-Z plane.
        /// </summary>
        public class HorizontalConverter : CoordinateConverter {
            public override Vector3 GridToWorldCenter(int x, int y, float cellSize, Vector3 origin) {
                return new Vector3(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f) + origin;
            }

            public override Vector3 GridToWorld(int x, int y, float cellSize, Vector3 origin) {
                return new Vector3(x, 0, y) * cellSize + origin;
            }

            public override Vector2Int WorldToGrid(Vector3 worldPosition, float cellSize, Vector3 origin) {
                Vector3 gridPosition = (worldPosition - origin) / cellSize;
                var x = Mathf.FloorToInt(gridPosition.x);
                var y = Mathf.FloorToInt(gridPosition.z);
                return new Vector2Int(x, y);
            }
        
            public override Vector3 Forward => -Vector3.up;
        }
    }
}
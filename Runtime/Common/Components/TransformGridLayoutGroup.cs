using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameDevKit
{
    [ExecuteAlways]
    public class TransformGridLayoutGroup : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private Alignment _alignment;
        [SerializeField] private Axis _startAxis;
        [SerializeField] private Vector2 _cellSize;
        [SerializeField] private Vector2 _spacing;
        [SerializeField] private bool _layoutInactiveChildren;

        public enum Axis { Horizontal, Vertical }
        public enum Alignment { UpperLeft, UpperRight, LowerLeft, LowerRight, MiddleCenter, MiddleCenterFixed }

        public readonly Vector2Int MinCoord = Vector2Int.zero;
        public Vector2Int MaxCoord => new(_gridSize.x - 1, _gridSize.y - 1);

        private readonly List<Transform> _layoutTargets = new();
        private bool _isDirty;

        public int CellCount => GridSize.Count();

        public Vector3 WorldSize => GridSize * (CellSize + Spacing);

        public Vector2Int GridSize
        {
            get => _gridSize;
            set => SetProperty(ref _gridSize, value);
        }

        public Axis StartAxis
        {
            get => _startAxis;
            set => SetProperty(ref _startAxis, value);
        }

        public Vector2 Spacing
        {
            get => _spacing;
            set => SetProperty(ref _spacing, value);
        }

        public Vector2 CellSize
        {
            get => _cellSize;
            set => SetProperty(ref _cellSize, value);
        }

        public bool LayoutInactiveChildren
        {
            get => _layoutInactiveChildren;
            set => SetProperty(ref _layoutInactiveChildren, value);
        }

        #region Unity Messages

        private void Update()
        {
            if (!_isDirty) { return; }
            _isDirty = false;
            Layout();
        }

        protected void OnEnable()
        {
            SetDirty();
        }

        protected virtual void OnTransformChildrenChanged()
        {
            SetDirty();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _gridSize.x = _gridSize.x.ClampMin(1);
            _gridSize.y = _gridSize.y.ClampMin(1);
            Layout();
        }
#endif

        #endregion

        private void UpdateLayoutTargets()
        {
            _layoutTargets.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child == null) { continue; }
                if (!child.gameObject.activeInHierarchy && !LayoutInactiveChildren) { continue; }

                _layoutTargets.Add(child);
            }
        }

        public void ForceLayout()
        {
            Layout();
            _isDirty = false;
        }

        private void Layout()
        {
            UpdateLayoutTargets();
            if (_layoutTargets.Count == 0) { return; }

            var displacement = Vector3.zero;

            if (_alignment is Alignment.MiddleCenter or Alignment.MiddleCenterFixed)
            {
                var minPos = GetCellCenterWorld(MinCoord);

                var maxCoord = MaxCoord;
                if (_alignment is Alignment.MiddleCenter)
                {
                    var lastIndex = _layoutTargets.Count - 1;
                    // if horizontal: (coordinate of lastIndex).y = yMax
                    // if vertical: (coordinate of lastIndex).x = xMax
                    var xMax = (GridSize.x - 1).ClampMax(lastIndex);
                    var yMax = (GridSize.y - 1).ClampMax(lastIndex);
                    switch (StartAxis)
                    {
                        case Axis.Horizontal:
                            yMax = GetCoordinate(lastIndex).y;
                            break;
                        case Axis.Vertical:
                            xMax = GetCoordinate(lastIndex).x;
                            break;
                    }
                    maxCoord = new Vector2Int(xMax, yMax);
                }

                var maxPos = GetCellCenterWorld(maxCoord);

                var xDistance = maxPos.x - minPos.x + CellSize.x;
                var yDistance = maxPos.y - minPos.y + CellSize.y;

                displacement.x = xDistance / 2;
                displacement.y = yDistance / 2;
            }

            for (int i = 0; i < _layoutTargets.Count; i++)
            {
                var child = _layoutTargets[i];
                var coord = GetCoordinate(i);
                var position = GetCellCenterWorld(coord);
                child.position = Vector3.Scale(position - displacement, transform.localScale);
            }

        }

        private Vector3 GetCellCenterWorld(Vector2Int coordinate)
        {
            var cellCenterPos = transform.position;
            if (coordinate == MinCoord) { return cellCenterPos; }

            var offset = (CellSize + Spacing) * coordinate;

            switch (_alignment)
            {
                case Alignment.UpperLeft:
                    offset.y = -offset.y;
                    break;
                case Alignment.UpperRight:
                    offset.x = -offset.x;
                    offset.y = -offset.y;
                    break;
                case Alignment.LowerLeft:
                    break;
                case Alignment.LowerRight:
                    offset.x = -offset.x;
                    break;
            }

            cellCenterPos += (Vector3)offset;
            return cellCenterPos;
        }

        public Vector2Int GetCoordinate(int index)
        {
            return StartAxis switch
            {
                Axis.Horizontal => new Vector2Int(index % GridSize.x, index / GridSize.x),
                Axis.Vertical => new Vector2Int(index / GridSize.y, index % GridSize.y),
                _ => default,
            };
        }


        protected bool SetProperty<T>(ref T currentValue, T newValue, bool setDirty = true)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            {
                return false;
            }
            currentValue = newValue;
            if (setDirty) { SetDirty(); }

            return true;
        }

        protected void SetDirty()
        {
            if (!isActiveAndEnabled) { return; }
            _isDirty = true;
        }

    }
}
// ----------------------------------------------------------------------------
// The MIT License
// InfiniteScroll https://github.com/mopsicus/infinite-scroll-unity
// Copyright (c) 2018-2021 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mopsicus.InfiniteScroll
{
    public interface IScrollItem { }

    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteScroll : MonoBehaviour, IDropHandler
    {

        /// <summary>
        /// Period for no-update list, if very fast add
        /// </summary>
        private const int UPDATE_TIME_DIFF = 500;

        /// <summary>
        /// Speed for scroll on move
        /// </summary>
        private const float SCROLL_SPEED = 50f;

        /// <summary>
        /// Duration for scroll move
        /// </summary>
        private const float SCROLL_DURATION = 0.25f;

        /// <summary>
        /// Load direction
        /// </summary>
        public enum Direction
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3
        }

        /// <summary>
        /// Event for get item height
        /// </summary>
        public Func<int, int> FnGetItemHeight;

        /// <summary>
        /// Event for get item width
        /// </summary>
        public Func<int, int> FnGetItemWidth;

        /// <summary>
        /// Callback on item fill
        /// </summary>
        public Action<int, IScrollItem> OnFill = delegate { };

        /// <summary>
        /// Callback on pull action
        /// </summary>
        public Action<Direction> OnPull = delegate { };

        [Header("Item settings")]
        /// <summary>
        /// Item list prefab
        /// </summary>
        public RectTransform Prefab;

        [Header("Padding")]
        /// <summary>
        /// Top padding
        /// </summary>
        public int TopPadding = 10;

        /// <summary>
        /// Bottom padding
        /// </summary>
        public int BottomPadding = 10;

        [Header("Padding")]
        /// <summary>
        /// Left padding
        /// </summary>
        public int LeftPadding = 10;

        /// <summary>
        /// Right padding
        /// </summary>
        public int RightPadding = 10;

        /// <summary>
        /// Spacing between items
        /// </summary>
        public int ItemSpacing = 2;

        [Header("Labels")]
        /// <summary>
        /// Label font asset
        /// </summary>
        public TMP_FontAsset LabelsFont;

        /// <summary>
        /// Pull top text label
        /// </summary>
        public string TopPullLabel = "Pull to refresh";

        /// <summary>
        /// Release top text label
        /// </summary>
        public string TopReleaseLabel = "Release to load";

        /// <summary>
        /// Pull bottom text label
        /// </summary>
        public string BottomPullLabel = "Pull to refresh";

        /// <summary>
        /// Release bottom text label
        /// </summary>
        public string BottomReleaseLabel = "Release to load";

        /// <summary>
        /// Pull left text label
        /// </summary>
        public string LeftPullLabel = "Pull to refresh";

        /// <summary>
        /// Release left text label
        /// </summary>
        public string LeftReleaseLabel = "Release to load";

        /// <summary>
        /// Pull right text label
        /// </summary>
        public string RightPullLabel = "Pull to refresh";

        /// <summary>
        /// Release right text label
        /// </summary>
        public string RightReleaseLabel = "Release to load";

        [Header("Directions")]
        /// <summary>
        /// Can we pull from top
        /// </summary>
        public bool IsPullTop = true;

        /// <summary>
        /// Can we pull from bottom
        /// </summary>
        public bool IsPullBottom = true;

        [Header("Directions")]
        /// <summary>
        /// Can we pull from left
        /// </summary>
        public bool IsPullLeft = true;

        /// <summary>
        /// Can we pull from right
        /// </summary>
        public bool IsPullRight = true;

        [Header("Offsets")]
        /// <summary>
        /// Coefficient when labels should action
        /// </summary>
        public float PullValue = 1.5f;

        /// <summary>
        /// Label position offset
        /// </summary>
        public float LabelOffset = 85f;

        [HideInInspector]
        /// <summary>
        /// Top label
        /// </summary>
        public TextMeshProUGUI TopLabel;

        [HideInInspector]
        /// <summary>
        /// Bottom label
        /// </summary>
        public TextMeshProUGUI BottomLabel;

        [HideInInspector]
        /// <summary>
        /// Left label
        /// </summary>
        public TextMeshProUGUI LeftLabel;

        [HideInInspector]
        /// <summary>
        /// Right label
        /// </summary>
        public TextMeshProUGUI RightLabel;

        /// <summary>
        /// Type of scroller
        /// </summary>
        [HideInInspector]
        public int Type;

        /// <summary>
        /// ScrollRect cache
        /// </summary>
        public ScrollRect ScrollRect { get; private set; }
        private RectTransform _content => ScrollRect.content;

        /// <summary>
        /// Container rect cache
        /// </summary>
        private Rect _container;

        private IScrollItem[] _items;

        /// <summary>
        /// All objects cache
        /// </summary>
        private RectTransform[] _views;

        /// <summary>
        /// State is can we pull from top
        /// </summary>
        private bool _isCanLoadUp;

        /// <summary>
        /// State is can we pull from bottom
        /// </summary>
        private bool _isCanLoadDown;

        /// <summary>
        /// State is can we pull from left
        /// </summary>
        private bool _isCanLoadLeft;

        /// <summary>
        /// State is can we pull from right
        /// </summary>
        private bool _isCanLoadRight;

        /// <summary>
        /// Previous position
        /// </summary>
        private int _previousPosition = -1;

        /// <summary>
        /// List items count
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Items heights cache
        /// </summary>
        private Dictionary<int, int> _heights = new();

        /// <summary>
        /// Items widths cache
        /// </summary>
        private Dictionary<int, int> _widths = new();

        /// <summary>
        /// Items positions cache
        /// </summary>
        private Dictionary<int, float> _positions = new();

        /// <summary>
        /// Last manual move time to end
        /// </summary>
        private DateTimeOffset _lastMoveTime;

        /// <summary>
        /// Cache for scroll position
        /// </summary>
        private float _previousScrollPosition;

        /// <summary>
        /// Cache position for prevent sides effects
        /// </summary>
        private int _saveStepPosition = -1;

        private void Awake()
        {
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        void Construct()
        {
            if (ScrollRect != null) { return; }

            ScrollRect = GetComponent<ScrollRect>();
            ScrollRect.onValueChanged.AddListener(OnScrollChange);
            _container = ScrollRect.viewport.rect;
            CreateLabels();

            Prefab.gameObject.SetActive(false);
        }

        /// <summary>
        /// Main loop to check items positions and heights
        /// </summary>
        void Update()
        {
            if (Count == 0) { return; }

            if (Type == 0)
            {
                UpdateVertical();
            }
            else
            {
                UpdateHorizontal();
            }
        }

        /// <summary>
        /// Main loop for vertical
        /// </summary>
        void UpdateVertical()
        {
            float _topPosition = _content.anchoredPosition.y - ItemSpacing;
            if (_topPosition <= 0f && _views[0].anchoredPosition.y < -TopPadding - 10f)
            {
                InitData(Count);
                return;
            }
            if (_topPosition < 0f)
            {
                return;
            }
            if (!_positions.ContainsKey(_previousPosition) || !_heights.ContainsKey(_previousPosition))
            {
                return;
            }
            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _heights[_previousPosition];
            int position = (_topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            int border = (int)(_positions[0] + _heights[0]);
            int step = (int)((_topPosition + _topPosition / 1.25f) / border);
            if (step != _saveStepPosition)
            {
                _saveStepPosition = step;
            }
            else
            {
                return;
            }
            if (position < 0 || _previousPosition == position || ScrollRect.velocity.y == 0f)
            {
                return;
            }
            if (position > _previousPosition)
            {
                if (position - _previousPosition > 1)
                {
                    position = _previousPosition + 1;
                }
                int newPosition = position % _views.Length;
                newPosition--;
                if (newPosition < 0)
                {
                    newPosition = _views.Length - 1;
                }
                int index = position + _views.Length - 1;
                if (index < Count)
                {
                    Vector2 pos = _views[newPosition].anchoredPosition;
                    pos.y = _positions[index];
                    _views[newPosition].anchoredPosition = pos;
                    Vector2 size = _views[newPosition].sizeDelta;
                    size.y = _heights[index];
                    _views[newPosition].sizeDelta = size;
                    _views[newPosition].name = index.ToString();
                    OnFill(index, _items[newPosition]);
                }
            }
            else
            {
                if (_previousPosition - position > 1)
                {
                    position = _previousPosition - 1;
                }
                int newIndex = position % _views.Length;
                Vector2 pos = _views[newIndex].anchoredPosition;
                pos.y = _positions[position];
                _views[newIndex].anchoredPosition = pos;
                Vector2 size = _views[newIndex].sizeDelta;
                size.y = _heights[position];
                _views[newIndex].sizeDelta = size;
                _views[newIndex].name = position.ToString();
                OnFill(position, _items[newIndex]);
            }
            _previousPosition = position;
        }

        /// <summary>
        /// Main loop for horizontal
        /// </summary>
        void UpdateHorizontal()
        {
            float _leftPosition = _content.anchoredPosition.x * -1f - ItemSpacing;
            if (_leftPosition <= 0f && _views[0].anchoredPosition.x < -LeftPadding - 10f)
            {
                InitData(Count);
                return;
            }
            if (_leftPosition < 0f)
            {
                return;
            }
            if (!_positions.ContainsKey(_previousPosition) || !_widths.ContainsKey(_previousPosition))
            {
                return;
            }
            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _widths[_previousPosition];
            int position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            int border = (int)(_positions[0] + _widths[0]);
            int step = (int)((_leftPosition + _leftPosition / 1.25f) / border);
            if (step != _saveStepPosition)
            {
                _saveStepPosition = step;
            }
            else
            {
                return;
            }
            if (position < 0 || _previousPosition == position || ScrollRect.velocity.x == 0f)
            {
                return;
            }
            if (position > _previousPosition)
            {
                if (position - _previousPosition > 1)
                {
                    position = _previousPosition + 1;
                }
                int newPosition = position % _views.Length;
                newPosition--;
                if (newPosition < 0)
                {
                    newPosition = _views.Length - 1;
                }
                int index = position + _views.Length - 1;
                if (index < Count)
                {
                    Vector2 pos = _views[newPosition].anchoredPosition;
                    pos.x = _positions[index];
                    _views[newPosition].anchoredPosition = pos;
                    Vector2 size = _views[newPosition].sizeDelta;
                    size.x = _widths[index];
                    _views[newPosition].sizeDelta = size;
                    _views[newPosition].name = index.ToString();
                    OnFill(index, _items[newPosition]);
                }
            }
            else
            {
                if (_previousPosition - position > 1)
                {
                    position = _previousPosition - 1;
                }
                int newIndex = position % _views.Length;
                Vector2 pos = _views[newIndex].anchoredPosition;
                pos.x = _positions[position];
                _views[newIndex].anchoredPosition = pos;
                Vector2 size = _views[newIndex].sizeDelta;
                size.x = _widths[position];
                _views[newIndex].sizeDelta = size;
                _views[newIndex].name = position.ToString();
                OnFill(position, _items[newIndex]);
            }
            _previousPosition = position;
        }

        /// <summary>
        /// Handler on scroller
        /// </summary>
        void OnScrollChange(Vector2 vector)
        {
            if (Type == 0)
            {
                ScrollChangeVertical(vector);
            }
            else
            {
                ScrollChangeHorizontal(vector);
            }
        }

        /// <summary>
        /// Handler on vertical scroll change
        /// </summary>
        void ScrollChangeVertical(Vector2 vector)
        {
            _isCanLoadUp = false;
            _isCanLoadDown = false;
            if (_views == null)
            {
                return;
            }
            float y = 0f;
            float z = 0f;
            bool isScrollable = (ScrollRect.verticalNormalizedPosition != 1f && ScrollRect.verticalNormalizedPosition != 0f);
            y = _content.anchoredPosition.y;
            if (isScrollable)
            {
                if (ScrollRect.verticalNormalizedPosition < 0f)
                {
                    z = y - _previousScrollPosition;
                }
                else
                {
                    _previousScrollPosition = y;
                }
            }
            else
            {
                z = y;
            }

            if (TopLabel != null)
            {
                if (y < -LabelOffset && IsPullTop)
                {
                    TopLabel.gameObject.SetActive(true);
                    TopLabel.text = TopPullLabel;
                    if (y < -LabelOffset * PullValue)
                    {
                        TopLabel.text = TopReleaseLabel;
                        _isCanLoadUp = true;
                    }
                }
                else
                {
                    TopLabel.gameObject.SetActive(false);
                }
            }

            if (BottomLabel != null)
            {
                if (z > LabelOffset && IsPullBottom)
                {
                    BottomLabel.gameObject.SetActive(true);
                    BottomLabel.text = BottomPullLabel;
                    if (z > LabelOffset * PullValue)
                    {
                        BottomLabel.text = BottomReleaseLabel;
                        _isCanLoadDown = true;
                    }
                }
                else
                {
                    BottomLabel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Handler on horizontal scroll change
        /// </summary>
        void ScrollChangeHorizontal(Vector2 vector)
        {
            _isCanLoadLeft = false;
            _isCanLoadRight = false;
            if (_views == null)
            {
                return;
            }
            float x = 0f;
            float z = 0f;
            bool isScrollable = (ScrollRect.horizontalNormalizedPosition != 1f && ScrollRect.horizontalNormalizedPosition != 0f);
            x = _content.anchoredPosition.x;
            if (isScrollable)
            {
                if (ScrollRect.horizontalNormalizedPosition > 1f)
                {
                    z = x - _previousScrollPosition;
                }
                else
                {
                    _previousScrollPosition = x;
                }
            }
            else
            {
                z = x;
            }

            if (LeftLabel != null)
            {
                if (x > LabelOffset && IsPullLeft)
                {
                    LeftLabel.gameObject.SetActive(true);
                    LeftLabel.text = LeftPullLabel;
                    if (x > LabelOffset * PullValue)
                    {
                        LeftLabel.text = LeftReleaseLabel;
                        _isCanLoadLeft = true;
                    }
                }
                else
                {
                    LeftLabel.gameObject.SetActive(false);
                }
            }

            if (RightLabel != null)
            {
                if (z < -LabelOffset && IsPullRight)
                {
                    RightLabel.gameObject.SetActive(true);
                    RightLabel.text = RightPullLabel;
                    if (z < -LabelOffset * PullValue)
                    {
                        RightLabel.text = RightReleaseLabel;
                        _isCanLoadRight = true;
                    }
                }
                else
                {
                    RightLabel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Handler on scroller drop pull
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            if (Type == 0)
            {
                DropVertical();
            }
            else
            {
                DropHorizontal();
            }
        }

        /// <summary>
        /// Handler on scroller vertical drop
        /// </summary>
        void DropVertical()
        {
            if (_isCanLoadUp)
            {
                OnPull(Direction.Top);
            }
            else if (_isCanLoadDown)
            {
                OnPull(Direction.Bottom);
            }
            _isCanLoadUp = false;
            _isCanLoadDown = false;
        }

        /// <summary>
        /// Handler on scroller horizontal drop
        /// </summary>
        void DropHorizontal()
        {
            if (_isCanLoadLeft)
            {
                OnPull(Direction.Left);
            }
            else if (_isCanLoadRight)
            {
                OnPull(Direction.Right);
            }
            _isCanLoadLeft = false;
            _isCanLoadRight = false;
        }

        /// <summary>
        /// Init list
        /// </summary>
        /// <param name="count">Items count</param>
        public void InitData(int count)
        {
            Construct();

            if (count < 1)
            {
                RecycleAll();
                return;
            }

            if (Type == 0)
            {
                InitVertical(count);
            }
            else
            {
                InitHorizontal(count);
            }
        }

        /// <summary>
        /// Init vertical list
        /// </summary>
        /// <param name="count">Item count</param>
        void InitVertical(int count)
        {
            float height = CalcSizesPositions(count);
            CreateViews(true);
            _previousPosition = 0;
            Count = count;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            Vector2 pos = _content.anchoredPosition;
            Vector2 size = Vector2.zero;
            pos.y = 0f;
            _content.anchoredPosition = pos;
            int y = TopPadding;
            bool showed = false;
            for (int i = 0; i < _views.Length; i++)
            {
                showed = i < count;
                _views[i].gameObject.SetActive(showed);
                if (i + 1 > Count)
                {
                    continue;
                }
                pos = _views[i].anchoredPosition;
                pos.y = _positions[i];
                pos.x = 0f;
                _views[i].anchoredPosition = pos;
                size = _views[i].sizeDelta;
                size.y = _heights[i];
                _views[i].sizeDelta = size;
                y += ItemSpacing + _heights[i];
                _views[i].name = i.ToString();
                OnFill(i, _items[i]);
            }
        }

        /// <summary>
        /// Init horizontal list
        /// </summary>
        /// <param name="count">Item count</param>
        void InitHorizontal(int count)
        {
            float width = CalcSizesPositions(count);
            CreateViews(false);
            _previousPosition = 0;
            Count = count;
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            Vector2 pos = _content.anchoredPosition;
            Vector2 size = Vector2.zero;
            pos.x = 0f;
            _content.anchoredPosition = pos;
            int x = LeftPadding;
            bool showed = false;
            for (int i = 0; i < _views.Length; i++)
            {
                showed = i < count;
                _views[i].gameObject.SetActive(showed);
                if (i + 1 > Count)
                {
                    continue;
                }
                pos = _views[i].anchoredPosition;
                pos.x = _positions[i];
                pos.y = 0f;
                _views[i].anchoredPosition = pos;
                size = _views[i].sizeDelta;
                size.x = _widths[i];
                _views[i].sizeDelta = size;
                x += ItemSpacing + _widths[i];
                _views[i].name = i.ToString();
                OnFill(i, _items[i]);
            }
        }

        /// <summary>
        /// Calc all items height and positions
        /// </summary>
        /// <returns>Common content height</returns>
        float CalcSizesPositions(int count)
        {
            return (Type == 0) ? CalcSizesPositionsVertical(count) : CalcSizesPositionsHorizontal(count);
        }

        /// <summary>
        /// Calc all items height and positions
        /// </summary>
        /// <returns>Common content height</returns>
        float CalcSizesPositionsVertical(int count)
        {
            _heights.Clear();
            _positions.Clear();
            float result = 0f;
            for (int i = 0; i < count; i++)
            {
                _heights[i] = FnGetItemHeight(i);
                _positions[i] = -(TopPadding + i * ItemSpacing + result);
                result += _heights[i];
            }
            result += TopPadding + BottomPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        /// <summary>
        /// Calc all items width and positions
        /// </summary>
        /// <returns>Common content width</returns>
        float CalcSizesPositionsHorizontal(int count)
        {
            _widths.Clear();
            _positions.Clear();
            float result = 0f;
            for (int i = 0; i < count; i++)
            {
                _widths[i] = FnGetItemWidth(i);
                _positions[i] = LeftPadding + i * ItemSpacing + result;
                result += _widths[i];
            }
            result += LeftPadding + RightPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        /// <summary>
        /// Update list after load new items
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        public void ApplyDataTo(int count, int newCount, Direction direction)
        {
            if (Type == 0)
            {
                ApplyDataToVertical(count, newCount, direction);
            }
            else
            {
                ApplyDataToHorizontal(count, newCount, direction);
            }
        }

        /// <summary>
        /// Update list after load new items for vertical scroller
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        void ApplyDataToVertical(int count, int newCount, Direction direction)
        {
            Count = count;
            if (Count <= _views.Length)
            {
                InitData(count);
                return;
            }
            float height = CalcSizesPositions(count);
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            Vector2 pos = _content.anchoredPosition;
            if (direction == Direction.Top)
            {
                float y = 0f;
                for (int i = 0; i < newCount; i++)
                {
                    y += _heights[i] + ItemSpacing;
                }
                pos.y = y;
                _previousPosition = newCount;
            }
            else
            {
                float h = 0f;
                for (int i = _heights.Count - 1; i >= _heights.Count - newCount; i--)
                {
                    h += _heights[i] + ItemSpacing;
                }
                pos.y = height - h - _container.height;
            }
            _content.anchoredPosition = pos;
            float _topPosition = _content.anchoredPosition.y - ItemSpacing;
            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _heights[_previousPosition];
            int position = (_topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            if (position < 0)
            {
                _previousPosition = 0;
                position = 1;
            }
            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = position % _views.Length;
                if (newIndex < 0)
                {
                    continue;
                }
                _views[newIndex].gameObject.SetActive(true);
                _views[newIndex].name = position.ToString();
                OnFill(position, _items[newIndex]);
                pos = _views[newIndex].anchoredPosition;
                pos.y = _positions[position];
                _views[newIndex].anchoredPosition = pos;
                Vector2 size = _views[newIndex].sizeDelta;
                size.y = _heights[position];
                _views[newIndex].sizeDelta = size;
                position++;
                if (position == Count)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Update list after load new items for horizontal scroller
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        void ApplyDataToHorizontal(int count, int newCount, Direction direction)
        {
            Count = count;
            if (Count <= _views.Length)
            {
                InitData(count);
                return;
            }
            float width = CalcSizesPositions(count);
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            Vector2 pos = _content.anchoredPosition;
            if (direction == Direction.Left)
            {
                float x = 0f;
                for (int i = 0; i < newCount; i++)
                {
                    x -= _widths[i] + ItemSpacing;
                }
                pos.x = x;
                _previousPosition = newCount;
            }
            else
            {
                float w = 0f;
                for (int i = _widths.Count - 1; i >= _widths.Count - newCount; i--)
                {
                    w += _widths[i] + ItemSpacing;
                }
                pos.x = -width + w + _container.width;
            }
            _content.anchoredPosition = pos;
            float _leftPosition = _content.anchoredPosition.x - ItemSpacing;
            float itemPosition = Mathf.Abs(_positions[_previousPosition]) + _widths[_previousPosition];
            int position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            if (position < 0)
            {
                _previousPosition = 0;
                position = 1;
            }
            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = position % _views.Length;
                if (newIndex < 0)
                {
                    continue;
                }
                _views[newIndex].gameObject.SetActive(true);
                _views[newIndex].name = position.ToString();
                OnFill(position, _items[newIndex]);
                pos = _views[newIndex].anchoredPosition;
                pos.x = _positions[position];
                _views[newIndex].anchoredPosition = pos;
                Vector2 size = _views[newIndex].sizeDelta;
                size.x = _widths[position];
                _views[newIndex].sizeDelta = size;
                position++;
                if (position == Count)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Update list after items delete
        /// </summary>
        /// <param name="index">Index to move from</param>
        /// <param name="height">New height</param>
        void MoveDataTo(int index, float height)
        {
            if (Type == 0)
            {
                MoveDataToVertical(index, height);
            }
            else
            {
                MoveDataToHorizontal(index, height);
            }
        }

        /// <summary>
        /// Update list after items delete for vertical scroller
        /// </summary>
        /// <param name="index">Index to move from</param>
        /// <param name="height">New height</param>
        void MoveDataToVertical(int index, float height)
        {
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            Vector2 pos = _content.anchoredPosition;
            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = index % _views.Length;
                _views[newIndex].name = index.ToString();
                if (index >= Count)
                {
                    _views[newIndex].gameObject.SetActive(false);
                    continue;
                }
                else
                {
                    _views[newIndex].gameObject.SetActive(true);
                    OnFill(index, _items[newIndex]);
                }
                pos = _views[newIndex].anchoredPosition;
                pos.y = _positions[index];
                _views[newIndex].anchoredPosition = pos;
                Vector2 size = _views[newIndex].sizeDelta;
                size.y = _heights[index];
                _views[newIndex].sizeDelta = size;
                index++;
            }
        }

        /// <summary>
        /// Update list after items delete for horizontal scroller
        /// </summary>
        /// <param name="index">Index to move from</param>
        /// <param name="width">New width</param>
        void MoveDataToHorizontal(int index, float width)
        {
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            Vector2 pos = _content.anchoredPosition;
            for (int i = 0; i < _views.Length; i++)
            {
                int newIndex = index % _views.Length;
                _views[newIndex].name = index.ToString();
                if (index >= Count)
                {
                    _views[newIndex].gameObject.SetActive(false);
                    continue;
                }
                else
                {
                    _views[newIndex].gameObject.SetActive(true);
                    OnFill(index, _items[newIndex]);
                }
                pos = _views[newIndex].anchoredPosition;
                pos.x = _positions[index];
                _views[newIndex].anchoredPosition = pos;
                Vector2 size = _views[newIndex].sizeDelta;
                size.x = _widths[index];
                _views[newIndex].sizeDelta = size;
                index++;
            }
        }

        /// <summary>
        /// Move scroll to side
        /// </summary>
        /// <param name="direction">Direction to move</param>
        public void MoveToSide(Direction direction)
        {
            var now = DateTimeOffset.UtcNow;
            if ((now - _lastMoveTime).TotalMilliseconds < UPDATE_TIME_DIFF)
            {
                return;
            }
            _lastMoveTime = now;
            StartCoroutine(MoveTo(direction));
        }

        /// <summary>
        /// Move coroutine
        /// </summary>
        /// <param name="direction">Direction to move</param>
        IEnumerator MoveTo(Direction direction)
        {
            float speed = SCROLL_SPEED;
            float start = 0f;
            float end = 0f;
            float timer = 0f;
            if (Type == 0)
            {
                start = ScrollRect.verticalNormalizedPosition;
                end = (direction == Direction.Bottom) ? 0f : 1f;
            }
            else
            {
                start = ScrollRect.horizontalNormalizedPosition;
                end = (direction == Direction.Left) ? 0f : 1f;
            }
            while (timer <= 1f)
            {
                speed = Mathf.Lerp(speed, 0f, timer);
                if (Type == 0)
                {
                    ScrollRect.verticalNormalizedPosition = Mathf.Lerp(start, end, timer);
                    ScrollRect.velocity = new Vector2(0f, (direction == Direction.Top) ? -speed : speed);
                }
                else
                {
                    ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, end, timer);
                    ScrollRect.velocity = new Vector2((direction == Direction.Left) ? speed : -speed, 0f);
                }
                timer += Time.deltaTime / SCROLL_DURATION;
                yield return null;
            }
            if (Type == 0)
            {
                ScrollRect.velocity = new Vector2(0f, (direction == Direction.Top) ? -SCROLL_SPEED : SCROLL_SPEED);
            }
            else
            {
                ScrollRect.velocity = new Vector2((direction == Direction.Left) ? SCROLL_SPEED : -SCROLL_SPEED, 0f);
            }
        }

        /// <summary>
        /// Disable all items in list
        /// </summary>
        public void RecycleAll()
        {
            Count = 0;
            if (_views == null) { return; }

            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Disable item
        /// </summary>
        /// <param name="index">Index in list data</param>
        public void Recycle(int index)
        {
            Count--;
            string name = index.ToString();
            float height = CalcSizesPositions(Count);
            for (int i = 0; i < _views.Length; i++)
            {
                if (string.CompareOrdinal(_views[i].name, name) == 0)
                {
                    _views[i].gameObject.SetActive(false);
                    MoveDataTo(i, height);
                    break;
                }
            }
        }

        /// <summary>
        /// Update visible items with new data
        /// </summary>
        public void UpdateVisible()
        {
            if (_views == null) { return; }

            bool showed = false;
            for (int i = 0; i < _views.Length; i++)
            {
                showed = i < Count;
                _views[i].gameObject.SetActive(showed);
                if (i + 1 > Count)
                {
                    continue;
                }
                int index = int.Parse(_views[i].name);
                OnFill(index, _items[i]);
            }
        }

        /// <summary>
        /// Clear views cache
        /// Needed to recreate views after Prefab change
        /// </summary>
        public void RefreshViews()
        {
            if (_views == null) { return; }

            foreach (var view in _views)
            {
                Destroy(view.gameObject);
            }

            _views = null;
            _items = null;

            if (Type == 0)
            {
                CreateViews(true);
            }
            else
            {
                CreateViews(false);
            }
        }

        private void CreateViews(bool isVertical)
        {
            if (_views != null) { return; }

            var sizes = isVertical ? _heights : _widths;

            if (sizes.Count < 1)
            {
                Debug.LogWarning("Should have more than 0 size!");
                return;
            }

            int size = 0;
            foreach (int item in sizes.Values)
            {
                size += item;
            }

            size /= sizes.Count;

            int fillCount = Mathf.RoundToInt((isVertical ? _container.height : _container.width) / size) + 4;

            _views = new RectTransform[fillCount];
            _items = new IScrollItem[fillCount];

            for (int i = 0; i < fillCount; i++)
            {
                var clone = Instantiate(Prefab, _content);
                clone.localScale = Vector3.one;
                clone.localPosition = Vector3.zero;

                clone.pivot = isVertical ? new Vector2(0.5f, 1f) : new Vector2(0f, 0.5f);
                clone.anchorMin = isVertical ? new Vector2(0f, 1f) : Vector2.zero;
                clone.anchorMax = isVertical ? Vector2.one : new Vector2(0f, 1f);
                clone.offsetMax = Vector2.zero;
                clone.offsetMin = Vector2.zero;
                _views[i] = clone;
                _items[i] = clone.GetComponent<IScrollItem>();
            }
        }

        /// <summary>
        /// Create labels
        /// </summary>
        void CreateLabels()
        {
            if (Type == 0)
            {
                CreateLabelsVertical();
            }
            else
            {
                CreateLabelsHorizontal();
            }
        }

        /// <summary>
        /// Create labels for vertical scroller
        /// </summary>
        void CreateLabelsVertical()
        {
            if (IsPullTop)
            {
                GameObject topText = new GameObject("TopLabel");
                topText.transform.SetParent(ScrollRect.viewport.transform);
                TopLabel = topText.AddComponent<TextMeshProUGUI>();
                TopLabel.font = LabelsFont;
                TopLabel.fontSize = 24;
                TopLabel.transform.localScale = Vector3.one;
                TopLabel.alignment = TextAlignmentOptions.Center;
                TopLabel.text = TopPullLabel;
                RectTransform rect = TopLabel.transform as RectTransform;
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = Vector2.one;
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = new Vector2(0f, -LabelOffset);
                rect.anchoredPosition3D = Vector3.zero;
                topText.SetActive(false);
            }

            if (IsPullBottom)
            {
                GameObject bottomText = new GameObject("BottomLabel");
                bottomText.transform.SetParent(ScrollRect.viewport.transform);
                BottomLabel = bottomText.AddComponent<TextMeshProUGUI>();
                BottomLabel.font = LabelsFont;
                BottomLabel.fontSize = 24;
                BottomLabel.transform.localScale = Vector3.one;
                BottomLabel.alignment = TextAlignmentOptions.Center;
                BottomLabel.text = BottomPullLabel;
                BottomLabel.transform.position = Vector3.zero;
                RectTransform rect = BottomLabel.transform as RectTransform;
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(1f, 0f);
                rect.offsetMax = new Vector2(0f, LabelOffset);
                rect.offsetMin = Vector2.zero;
                rect.anchoredPosition3D = Vector3.zero;
                bottomText.SetActive(false);
            }
        }

        /// <summary>
        /// Create labels for horizontal scroller
        /// </summary>
        void CreateLabelsHorizontal()
        {

            if (IsPullLeft)
            {
                GameObject leftText = new GameObject("LeftLabel");
                leftText.transform.SetParent(ScrollRect.viewport.transform);
                LeftLabel = leftText.AddComponent<TextMeshProUGUI>();
                LeftLabel.font = LabelsFont;
                LeftLabel.fontSize = 24;
                LeftLabel.transform.localScale = Vector3.one;
                LeftLabel.alignment = TextAlignmentOptions.Center;
                LeftLabel.text = LeftPullLabel;
                RectTransform rect = LeftLabel.transform as RectTransform;
                rect.pivot = new Vector2(0f, 0.5f);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(0f, 1f);
                rect.offsetMax = Vector2.zero;
                rect.offsetMin = new Vector2(-LabelOffset * 2, 0f);
                rect.anchoredPosition3D = Vector3.zero;
                leftText.SetActive(false);
            }

            if (IsPullRight)
            {
                GameObject rightText = new GameObject("RightLabel");
                rightText.transform.SetParent(ScrollRect.viewport.transform);
                RightLabel = rightText.AddComponent<TextMeshProUGUI>();
                RightLabel.font = LabelsFont;
                RightLabel.fontSize = 24;
                RightLabel.transform.localScale = Vector3.one;
                RightLabel.alignment = TextAlignmentOptions.Center;
                RightLabel.text = RightPullLabel;
                RightLabel.transform.position = Vector3.zero;
                RectTransform rect = RightLabel.transform as RectTransform;
                rect.pivot = new Vector2(1f, 0.5f);
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = Vector3.one;
                rect.offsetMax = new Vector2(LabelOffset * 2, 0f);
                rect.offsetMin = Vector2.zero;
                rect.anchoredPosition3D = Vector3.zero;
                rightText.SetActive(false);
            }
        }

    }

}

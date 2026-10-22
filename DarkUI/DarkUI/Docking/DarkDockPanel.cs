using DarkUI.Config;
using DarkUI.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DarkUI.Docking
{
    public class DarkDockPanel : UserControl
    {
        #region Event Region

        public event EventHandler<DockContentEventArgs> ActiveContentChanged;
        public event EventHandler<DockContentEventArgs> ContentAdded;
        public event EventHandler<DockContentEventArgs> ContentRemoved;

        #endregion

        #region Field Region

        private readonly List<DarkDockContent> _contents;
        private readonly Dictionary<DarkDockArea, DarkDockRegion> _regions;

        private bool _prioritizeLeft = true;
        private bool _prioritizeRight = true;
        private DarkDockContent _activeContent;
        private bool _switchingContent;
        private bool _isBulkUpdating;

        #endregion

        #region Property Region

        [DefaultValue(true)]
        public bool PrioritizeLeft
        {
            get { return _prioritizeLeft; }
            set
            {
                _prioritizeLeft = value;
                AddRegions();
            }
        }

        [DefaultValue(true)]
        public bool PrioritizeRight
        {
            get { return _prioritizeRight; }
            set
            {
                _prioritizeRight = value;
                AddRegions();
            }
        }

        [DefaultValue(false)]
        public bool EqualizeGroupSizes { get; set; } = false;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockContent ActiveContent
        {
            get { return _activeContent; }
            set
            {
                // Don't let content visibility changes re-trigger event
                if (_switchingContent)
                    return;

                _switchingContent = true;

                _activeContent = value;

                ActiveGroup = _activeContent.DockGroup;
                ActiveRegion = ActiveGroup.DockRegion;

                foreach (var region in _regions.Values)
                    region.Redraw();

                ActiveContentChanged?.Invoke(this, new DockContentEventArgs(_activeContent));

                _switchingContent = false;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockRegion ActiveRegion { get; internal set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockGroup ActiveGroup { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DarkDockContent ActiveDocument
        {
            get
            {
                return _regions[DarkDockArea.Document].ActiveDocument;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockContentDragFilter DockContentDragFilter { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockResizeFilter DockResizeFilter { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<DarkDockSplitter> Splitters { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MouseButtons MouseButtonState
        {
            get
            {
                var buttonState = MouseButtons;
                return buttonState;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<DarkDockArea, DarkDockRegion> Regions
        {
            get
            {
                return _regions;
            }
        }

        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackColor
        {
            get { return Colors.GreyBackground; }
            set { base.BackColor = Colors.GreyBackground; }
        }

        #endregion

        #region Constructor Region

        internal bool IsBulkUpdating => _isBulkUpdating;

        public DarkDockPanel()
        {
            Splitters = new List<DarkDockSplitter>();
            DockContentDragFilter = new DockContentDragFilter(this);
            DockResizeFilter = new DockResizeFilter(this);

            _regions = new Dictionary<DarkDockArea, DarkDockRegion>();
            _contents = new List<DarkDockContent>();

            CreateRegions(); 
            
            base.BackColor = Colors.GreyBackground;
        }

        #endregion

        #region Method Region

        public void AddContent(DarkDockContent dockContent)
        {
            AddContent(dockContent, null);
        }

        public void AddContent(DarkDockContent dockContent, DarkDockGroup dockGroup)
        {
            if (_contents.Contains(dockContent))
                RemoveContent(dockContent);

            dockContent.DockPanel = this;
            _contents.Add(dockContent);

            if (dockGroup != null)
                dockContent.DockArea = dockGroup.DockArea;

            if (dockContent.DockArea == DarkDockArea.None)
                dockContent.DockArea = dockContent.DefaultDockArea;

            var region = _regions[dockContent.DockArea];
            region.AddContent(dockContent, dockGroup);

            ContentAdded?.Invoke(this, new DockContentEventArgs(dockContent));

            if (!_isBulkUpdating)
                dockContent.Select();
        }

        public void InsertContent(DarkDockContent dockContent, DarkDockGroup dockGroup, DockInsertType insertType)
        {
            if (_contents.Contains(dockContent))
                RemoveContent(dockContent);

            dockContent.DockPanel = this;
            _contents.Add(dockContent);

            dockContent.DockArea = dockGroup.DockArea;

            var region = _regions[dockGroup.DockArea];
            region.InsertContent(dockContent, dockGroup, insertType);

            ContentAdded?.Invoke(this, new DockContentEventArgs(dockContent));

            dockContent.Select();
        }

        public void RemoveContent()
        {
            if (_contents.Count == 0)
                return;

            BeginBulkUpdate();

            while (_contents.Count > 0)
                RemoveContent(_contents.First());

            EndBulkUpdate();
        }

        public void RemoveContent(DarkDockContent dockContent)
        {
            if (!_contents.Contains(dockContent))
                return;

            dockContent.DockPanel = null;
            _contents.Remove(dockContent);

            var region = _regions[dockContent.DockArea];
            region.RemoveContent(dockContent);

            ContentRemoved?.Invoke(this, new DockContentEventArgs(dockContent));
        }

        public bool ContainsContent(DarkDockContent dockContent)
        {
            return _contents.Contains(dockContent);
        }

        public List<DarkDockContent> GetDocuments()
        {
            return _regions[DarkDockArea.Document].GetContents();
        }

        private void CreateRegions()
        {
            var documentRegion = new DarkDockRegion(this, DarkDockArea.Document);
            _regions.Add(DarkDockArea.Document, documentRegion);

            var leftRegion = new DarkDockRegion(this, DarkDockArea.Left);
            _regions.Add(DarkDockArea.Left, leftRegion);

            var rightRegion = new DarkDockRegion(this, DarkDockArea.Right);
            _regions.Add(DarkDockArea.Right, rightRegion);

            var bottomRegion = new DarkDockRegion(this, DarkDockArea.Bottom);
            _regions.Add(DarkDockArea.Bottom, bottomRegion);

            AddRegions();

            // Create tab index for intuitive tabbing order
            documentRegion.TabIndex = 0;
            rightRegion.TabIndex = 1;
            bottomRegion.TabIndex = 2;
            leftRegion.TabIndex = 3;
        }

        private void AddRegions()
        {
            Controls.Clear();

            Controls.Add(_regions[DarkDockArea.Document]);

            if (PrioritizeLeft && PrioritizeRight)
            {
                Controls.Add(_regions[DarkDockArea.Bottom]);
                Controls.Add(_regions[DarkDockArea.Left]);
                Controls.Add(_regions[DarkDockArea.Right]);
            }
            else if (PrioritizeLeft)
            {
                Controls.Add(_regions[DarkDockArea.Right]);
                Controls.Add(_regions[DarkDockArea.Bottom]);
                Controls.Add(_regions[DarkDockArea.Left]);
            }
            else if (PrioritizeRight)
            {
                Controls.Add(_regions[DarkDockArea.Left]);
                Controls.Add(_regions[DarkDockArea.Bottom]);
                Controls.Add(_regions[DarkDockArea.Right]);
            }
            else
            {
                Controls.Add(_regions[DarkDockArea.Left]);
                Controls.Add(_regions[DarkDockArea.Right]);
                Controls.Add(_regions[DarkDockArea.Bottom]);
            }
        }

        public void DragContent(DarkDockContent content)
        {
            DockContentDragFilter.StartDrag(content);
        }

        #endregion

        #region Serialization Region

        public DockPanelState GetDockPanelState()
        {
            var state = new DockPanelState();

            state.Regions.Add(new DockRegionState(DarkDockArea.Document));
            state.Regions.Add(new DockRegionState(DarkDockArea.Left, _regions[DarkDockArea.Left].Size));
            state.Regions.Add(new DockRegionState(DarkDockArea.Right, _regions[DarkDockArea.Right].Size));
            state.Regions.Add(new DockRegionState(DarkDockArea.Bottom, _regions[DarkDockArea.Bottom].Size));

            var groupStates = new Dictionary<DarkDockGroup, DockGroupState>();

            var orderedContent = _contents.OrderBy(c => c.Order);
            foreach (var content in orderedContent)
            {
                foreach (var region in state.Regions)
                {
                    if (region.Area == content.DockArea)
                    {
                        DockGroupState groupState;

                        if (groupStates.ContainsKey(content.DockGroup))
                        {
                            groupState = groupStates[content.DockGroup];
                        }
                        else
                        {
                            groupState = new DockGroupState();
                            region.Groups.Add(groupState);
                            groupStates.Add(content.DockGroup, groupState);
                        }

                        groupState.Contents.Add(content.SerializationKey);

                        groupState.VisibleContent = content.DockGroup.VisibleContent.SerializationKey;
                        groupState.Order = content.DockGroup.Order;
                        groupState.Size = content.DockGroup.Size;
                    }
                }
            }

            return state;
        }

        public void RestoreDockPanelState(DockPanelState state, Func<string, DarkDockContent> getContentBySerializationKey)
        {
            BeginBulkUpdate();

            foreach (var region in state.Regions.OrderByDescending(r => r.Area))
            {
                switch (region.Area)
                {
                    case DarkDockArea.Left:
                        _regions[DarkDockArea.Left].Size = region.Size;
                        break;
                    case DarkDockArea.Right:
                        _regions[DarkDockArea.Right].Size = region.Size;
                        break;
                    case DarkDockArea.Bottom:
                        _regions[DarkDockArea.Bottom].Size = region.Size;
                        break;
                }

                region.Groups.Sort(delegate (DockGroupState a, DockGroupState b) { return a.Order.CompareTo(b.Order); });

                foreach (var group in region.Groups)
                {
                    DarkDockContent previousContent = null;
                    DarkDockContent visibleContent = null;

                    foreach (var contentKey in group.Contents)
                    {
                        var content = getContentBySerializationKey(contentKey);

                        if (content == null)
                            continue;

                        content.DockArea = region.Area;

                        if (previousContent == null)
                            AddContent(content);
                        else
                            AddContent(content, previousContent.DockGroup);

                        previousContent = content;

                        if (group.VisibleContent == contentKey)
                        {
                            visibleContent = content;
                        }
                    }

                    if (visibleContent != null)
                    {
                        visibleContent.Select();
                        visibleContent.DockGroup.Size = group.Size;
                    }

                }
            }

            EndBulkUpdate();
        }

        private void BeginBulkUpdate()
        {
            _isBulkUpdating = true;

            SuspendLayout();

            foreach (var region in _regions.Values)
                region.SuspendLayout();

            if (IsHandleCreated)
                Native.SendMessage(Handle, (uint)WM.SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        private void EndBulkUpdate()
        {
            foreach (var region in _regions.Values)
                region.FinalizeLayout();

            _isBulkUpdating = false;

            foreach (var region in _regions.Values)
                region.ResumeLayout(true);

            ResumeLayout(true);

            if (IsHandleCreated)
            {
                Native.SendMessage(Handle, (uint)WM.SETREDRAW, (IntPtr)1, IntPtr.Zero);
                Invalidate(true);
            }
        }

        #endregion
    }
}
    

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace WinClip
{
    public class WinClipApplicationContext : ApplicationContext
    {
        private readonly ClipboardManager _clipboardManager;
        private readonly ClipboardHistory _clipboardHistory;
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;

        public WinClipApplicationContext()
        {
            // クリップボード履歴と監視を開始
            _clipboardHistory = new ClipboardHistory();
            _clipboardManager = new ClipboardManager();
            _clipboardManager.ClipboardContentChanged += OnClipboardContentChanged;

            // タスクバーアイコンとメニューの初期化
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // 独自のアイコンに置き換える
                Visible = true
            };

            _contextMenu = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip = _contextMenu;

            // コンテキストメニューに終了アイテムを追加
            var exitItem = new ToolStripMenuItem("終了");
            exitItem.Click += (sender, e) => Application.Exit();
            _contextMenu.Items.Add(exitItem);

            // アプリケーション終了時の処理
            Application.ApplicationExit += OnApplicationExit;
        }

        private void OnClipboardContentChanged(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    _clipboardHistory.Add(clipboardText);
                    UpdateContextMenu();
                }
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            _clipboardManager.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        private void UpdateContextMenu()
        {
            // 履歴アイテムより下にあるアイテム（区切り線、終了）を退避
            var itemsToKeep = _contextMenu.Items.OfType<ToolStripItem>()
                .Where(item => item.Tag?.ToString() != "HistoryItemTag").ToList();
            
            _contextMenu.Items.Clear();

            // 新しい履歴アイテムを追加
            foreach (var historyItem in _clipboardHistory.History)
            {
                var menuItem = new ToolStripMenuItem();
                menuItem.Text = historyItem.Length > 20 ? historyItem.Substring(0, 20) + "..." : historyItem;
                menuItem.Tag = historyItem;
                menuItem.Click += HistoryItem_Click;
                _contextMenu.Items.Add(menuItem);
            }
            
            // 区切り線と終了メニューを追加し直す
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.AddRange(itemsToKeep.ToArray());
        }

        private void HistoryItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string textToCopy)
            {
                Clipboard.SetText(textToCopy);
            }
        }
    }
}
using Microsoft.Windows.Sdk;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PInvokeFolderOpener.CsWin32
{
    /// <summary>
    /// FolderBrowserDialog クラスは、フォルダーを選択する機能を提供するクラスです。
    /// <para>
    /// <see cref="Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialog"/> クラスを利用したフォルダーの選択に近い機能を提供します。
    /// </para>
    /// </summary>
    public sealed class FolderBrowserDialog
    {
        /// <summary>
        /// <see cref="Result"/> 列挙型は、ダイアログ ボックスの戻り値を示す識別子を表します。
        /// </summary>
        public enum Result : int
        {
            /// <summary>
            /// ダイアログ ボックスの戻り値は Nothing です。モーダル ダイアログ ボックスの実行が継続します。
            /// </summary>
            None = 0,
            /// <summary>
            /// ダイアログ ボックスの戻り値は OK です。
            /// </summary>
            OK = 1,
            /// <summary>
            /// ダイアログ ボックスの戻り値は Cancel です。
            /// </summary>
            Cancel = 2,
            /// <summary>
            /// ダイアログ ボックスの戻り値は Abort です。
            /// </summary>
            Abort = 3,
            /// <summary>
            /// ダイアログ ボックスの戻り値は Retry です。
            /// </summary>
            //Retry = 4,
            /// <summary>
            /// ダイアログ ボックスの戻り値は Ignore です。
            /// </summary>
            //Ignore = 5,
            /// <summary>
            /// ダイアログ ボックスの戻り値は Yes です。
            /// </summary>
            //Yes = 6,
            /// <summary>
            /// ダイアログ ボックスの戻り値は No です。
            /// </summary>
            //No = 7
        }

        // ◆自動生成できなかった（GUIDの根拠も分からなかった）
        [ComImport]
        [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialogInternal { }

        /// <summary>
        /// ユーザーによって選択されたフォルダーのパスを取得または設定します。
        /// 未選択時 や キャンセル時 は string.Empty が入ります。
        /// </summary>
        public string SelectedPath { get; private set; }

        /// <summary>
        /// ダイアログ上に表示されるタイトルのテキストを取得または設定します。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// ダイアログオープン時の初期PATH
        /// </summary>
        public string InitializedPath { get; }

        public FolderBrowserDialog(string title = "", string initializedPath = "")
        {
            SelectedPath = "";
            Title = string.IsNullOrEmpty(title) ? "Folder Select Dialog" : title;
            InitializedPath = initializedPath;
        }

        public Result ShowDialog(Window windowOwner)
        {
            var handle = new WindowInteropHelper(windowOwner).Handle;
            return ShowDialog(new HWND(handle));
        }

        public Result ShowDialog() => ShowDialog(new HWND(IntPtr.Zero));

        private unsafe Result ShowDialog(HWND owner)
        {
            SelectedPath = "";
            if (new FileOpenDialogInternal() is not IFileOpenDialog dialog) throw new NullReferenceException(nameof(dialog));

            try
            {
                dialog.SetOptions((uint)(_FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS | _FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM));
                dialog.SetTitle(Title);

                if (Directory.Exists(InitializedPath))
                {
                    uint attributes = 0;
                    ITEMIDLIST itemIdList;
                    ITEMIDLIST* pItemIdList = &itemIdList;

                    if (PInvoke.SHILCreateFromPath(InitializedPath, out pItemIdList, &attributes).Succeeded)
                    {
                        if (PInvoke.SHCreateShellItem(default(ITEMIDLIST), default, *pItemIdList, out IShellItem item0).Succeeded)
                        {
                            dialog.SetFolder(item0);
                        }

                        // ◆以下の必要性を確認する → 必要なさそう。ITEMIDLIST (struct) の唯一のメンバの SHITEMID も struct (on stack) なので解放いらんはず。
                        //if (itemIdList != IntPtr.Zero) Marshal.FreeCoTaskMem(idl);
                    }
                }

#if false
                // ◆本来はこちらの実装が正しいと思うけど、CsWin32 の IFileOpenDialog.Show が
                //   HRESULT を返さないので(void戻り値なので)、しゃーなしで else側 の実装にした。
                var hr = dialog.Show(owner);
                if (hr == Constants.ERROR_CANCELLED) return Result.Cancel;
                if (hr != new HRESULT(0)) return Result.Abort;
#else
                try
                {
                    // フォルダ選択ダイアログでキャンセルボタンを押すと COMException が発生する
                    dialog.Show(owner);
                }
                catch (COMException) { return Result.Cancel; }
#endif

                dialog.GetResult(out IShellItem item);
                if (item is null) return Result.Abort;

                item.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out PWSTR selectedPath);
                SelectedPath = selectedPath.ToString();
                return Result.OK;
            }
            finally
            {
                // ◆以下の必要性を確認する → これはCsWin32 と関係なく必要。
                Marshal.FinalReleaseComObject(dialog);
            }
        }

    }
}

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AESApp;

public sealed class MainForm : Form
{
    private byte[]? _key;
    private readonly TextBox _plain = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox _cipher = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly Label _status = new() { AutoSize = true, Text = "Khóa: chưa nạp" };
    private readonly ProgressBar _progress = new() { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1000 };
    private readonly Label _fileInfo = new() { AutoSize = true, Text = "Sẵn sàng" };

    public MainForm()
    {
        Text = "Phòng thí nghiệm mã hóa AES-256";
        Width = 1000; Height = 760; MinimumSize = new Size(850, 620);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildTextTab());
        tabs.TabPages.Add(BuildFileTab());
        Controls.Add(tabs);
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(10) };
        footer.Controls.Add(_status);
        Controls.Add(footer);
    }

    private TabPage BuildTextTab()
    {
        var page = new TabPage("Mã hóa văn bản");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(new Label { Text = "Văn bản gốc (UTF-8)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        layout.Controls.Add(new Label { Text = "Bản mã (Base64: nonce + tag + dữ liệu)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, 0);
        layout.Controls.Add(_plain, 0, 1); layout.Controls.Add(_cipher, 1, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        actions.Controls.Add(Button("Mã hóa văn bản", (_, _) => Guard(() => _cipher.Text = AesService.EncryptText(_plain.Text, NeedKey()))));
        actions.Controls.Add(Button("Giải mã văn bản", (_, _) => Guard(() => _plain.Text = AesService.DecryptText(_cipher.Text.Trim(), NeedKey()))));
        actions.Controls.Add(Button("Sao chép bản mã", (_, _) => { if (_cipher.Text.Length > 0) Clipboard.SetText(_cipher.Text); }));
        layout.Controls.Add(actions, 0, 2); layout.SetColumnSpan(actions, 2);
        layout.Controls.Add(new Label { Text = "Dán bản mã Base64 vào ô bên phải để giải mã. Sai khóa hoặc dữ liệu bị sửa sẽ khiến xác thực thất bại.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 3)!, 2);
        var keyButtons = KeyButtons();
        layout.Controls.Add(keyButtons, 0, 4); layout.SetColumnSpan(keyButtons, 2);
        page.Controls.Add(layout);
        return page;
    }
    private TabPage BuildFileTab()
    {
        var page = new TabPage("Mã hóa tệp lớn");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1, RowCount = 5 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(KeyButtons(), 0, 0);
        var encrypt = new FlowLayoutPanel { Dock = DockStyle.Fill };
        encrypt.Controls.Add(Button("Chọn tệp nguồn và mã hóa", (_, _) => EncryptFile()));
        encrypt.Controls.Add(new Label { Text = "Tạo tệp .aesgcm; tệp nguồn không bị thay đổi.", AutoSize = true, Padding = new Padding(4, 9, 0, 0) });
        layout.Controls.Add(encrypt, 0, 1);
        var decrypt = new FlowLayoutPanel { Dock = DockStyle.Fill };
        decrypt.Controls.Add(Button("Chọn tệp .aesgcm và giải mã", (_, _) => DecryptFile()));
        decrypt.Controls.Add(new Label { Text = "Giải mã đồng thời xác thực từng khối dữ liệu.", AutoSize = true, Padding = new Padding(4, 9, 0, 0) });
        layout.Controls.Add(decrypt, 0, 2);
        var compare = new FlowLayoutPanel { Dock = DockStyle.Fill };
        compare.Controls.Add(Button("So sánh hai tệp (SHA-256)", (_, _) => CompareFiles()));
        compare.Controls.Add(new Label { Text = "Dùng để so sánh tệp gốc với tệp đã giải mã.", AutoSize = true, Padding = new Padding(4, 9, 0, 0) });
        layout.Controls.Add(compare, 0, 3);
        var results = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, Height = 80 };
        results.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        results.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        results.Controls.Add(_progress, 0, 0); results.Controls.Add(_fileInfo, 0, 1);
        layout.Controls.Add(results, 0, 4);
        page.Controls.Add(layout);
        return page;
    }

    private Control KeyButtons()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        panel.Controls.Add(Button("Tạo khóa AES-256", (_, _) => Guard(() => { _key = AesService.NewKey(); _status.Text = "Khóa đã tạo trong bộ nhớ (chưa lưu)"; })));
        panel.Controls.Add(Button("Lưu khóa ra tệp", (_, _) => SaveKey()));
        panel.Controls.Add(Button("Nạp khóa từ tệp", (_, _) => ImportKey()));
        return panel;
    }

    private static Button Button(string text, EventHandler handler)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 32, Padding = new Padding(8, 2, 8, 2), Margin = new Padding(4) };
        b.Click += handler;
        return b;
    }

    private byte[] NeedKey() => _key ?? throw new InvalidOperationException("Hãy tạo khóa hoặc nạp tệp khóa AES trước.");
    private void SaveKey()
    {
        Guard(() =>
        {
            byte[] key = NeedKey();
            using var dialog = new SaveFileDialog { Filter = "AES key (*.key)|*.key|All files (*.*)|*.*", FileName = "aes256.key" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                AesService.SaveKey(dialog.FileName, key);
                _status.Text = "Đã lưu khóa: " + dialog.FileName;
            }
        });
    }

    private void ImportKey()
    {
        Guard(() =>
        {
            using var dialog = new OpenFileDialog { Filter = "AES key (*.key)|*.key|All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _key = AesService.LoadKey(dialog.FileName);
                _status.Text = "Đã nạp khóa: " + dialog.FileName;
            }
        });
    }

    private void EncryptFile()
    {
        Guard(() =>
        {
            byte[] key = NeedKey();
            using var input = new OpenFileDialog { Title = "Chọn tệp nguồn" };
            if (input.ShowDialog(this) != DialogResult.OK) return;
            using var output = new SaveFileDialog { Title = "Lưu tệp đã mã hóa", Filter = "AES-GCM file (*.aesgcm)|*.aesgcm|All files (*.*)|*.*", FileName = Path.GetFileName(input.FileName) + ".aesgcm" };
            if (output.ShowDialog(this) != DialogResult.OK) return;
            RunFileOperation(input.FileName, output.FileName, key, true);
        });
    }

    private void DecryptFile()
    {
        Guard(() =>
        {
            byte[] key = NeedKey();
            using var input = new OpenFileDialog { Title = "Chọn tệp .aesgcm đã mã hóa", Filter = "AES-GCM file (*.aesgcm)|*.aesgcm|All files (*.*)|*.*" };
            if (input.ShowDialog(this) != DialogResult.OK) return;
            using var output = new SaveFileDialog { Title = "Lưu tệp đã giải mã", FileName = Path.GetFileNameWithoutExtension(input.FileName) + ".decrypted" };
            if (output.ShowDialog(this) != DialogResult.OK) return;
            RunFileOperation(input.FileName, output.FileName, key, false);
        });
    }

    private async void RunFileOperation(string source, string target, byte[] key, bool encrypt)
    {
        bool samePath = string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase);
        var watch = Stopwatch.StartNew();
        long total = new FileInfo(source).Length;
        _progress.Value = 0; _fileInfo.Text = "Đang xử lý...";
        try
        {
            if (samePath) throw new InvalidOperationException("Đường dẫn tệp đầu ra phải khác tệp nguồn.");
            Action<long> progress = done =>
            {
                int value = total == 0 ? 1000 : (int)Math.Clamp(done * 1000.0 / total, 0, 1000);
                if (InvokeRequired) BeginInvoke(new Action(() => _progress.Value = value)); else _progress.Value = value;
            };
            await Task.Run(() =>
            {
                if (encrypt) AesService.EncryptFile(source, target, key, progress);
                else AesService.DecryptFile(source, target, key, progress);
            });
            watch.Stop();
            _progress.Value = 1000;
            double seconds = Math.Max(watch.Elapsed.TotalSeconds, 0.000001);
            double mbps = total / 1024d / 1024d / seconds;
            _fileInfo.Text = $"{(encrypt ? "Mã hóa" : "Giải mã")} hoàn tất | {total:N0} byte đầu vào | {watch.Elapsed.TotalSeconds:F3} giây | {mbps:F2} MiB/s";
            MessageBox.Show(this, _fileInfo.Text + Environment.NewLine + "Tệp đầu ra: " + target, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            if (!samePath && File.Exists(target)) File.Delete(target);
            MessageBox.Show(this, ex.Message, "Thao tác tệp thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    private void CompareFiles()
    {
        Guard(() =>
        {
            using var a = new OpenFileDialog { Title = "Chọn tệp gốc/nguồn" };
            if (a.ShowDialog(this) != DialogResult.OK) return;
            using var b = new OpenFileDialog { Title = "Chọn tệp đã giải mã" };
            if (b.ShowDialog(this) != DialogResult.OK) return;
            var watch = Stopwatch.StartNew();
            bool bytesSame = FilesEqual(a.FileName, b.FileName);
            string hashA = HashFile(a.FileName);
            string hashB = HashFile(b.FileName);
            watch.Stop();
            bool same = bytesSame && string.Equals(hashA, hashB, StringComparison.Ordinal);
            _fileInfo.Text = $"So sánh từng byte: {(bytesSame ? "TRÙNG KHỚP" : "KHÁC NHAU")} | SHA-256: {(same ? "TRÙNG KHỚP" : "KHÁC NHAU")} | {watch.Elapsed.TotalSeconds:F3} giây";
            MessageBox.Show(this, $"Số byte tệp gốc: {new FileInfo(a.FileName).Length:N0}\nSố byte tệp đã giải mã: {new FileInfo(b.FileName).Length:N0}\nSo sánh từng byte: {(bytesSame ? "TRÙNG KHỚP" : "KHÁC NHAU")}\nSHA-256 tệp gốc: {hashA}\nSHA-256 tệp giải mã: {hashB}\nKết quả: {(same ? "Hai tệp giống hệt nhau" : "Hai tệp khác nhau; hãy kiểm tra tệp hoặc khóa")}", "So sánh tệp");
        });
    }

    private static bool FilesEqual(string first, string second)
    {
        var infoA = new FileInfo(first);
        var infoB = new FileInfo(second);
        if (infoA.Length != infoB.Length) return false;
        using var a = new FileStream(first, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        using var b = new FileStream(second, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        byte[] ba = new byte[1024 * 1024];
        byte[] bb = new byte[1024 * 1024];
        while (true)
        {
            int na = a.Read(ba, 0, ba.Length);
            int nb = b.Read(bb, 0, bb.Length);
            if (na != nb) return false;
            if (na == 0) return true;
            if (!ba.AsSpan(0, na).SequenceEqual(bb.AsSpan(0, nb))) return false;
        }
    }

    private static string HashFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private void Guard(Action action)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace RSAApp;

public sealed class MainForm : Form
{
    private readonly RsaService _rsa = new();
    private readonly TextBox _input = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _hashInput = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _cipher = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _hash = new() { Multiline = true, Dock = DockStyle.Fill, ReadOnly = true };
    private readonly TextBox _signature = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _pemView = new() { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Both, WordWrap = false };
    private readonly Label _byteCount = new() { AutoSize = true };
    private readonly Label _keyState = new() { AutoSize = true, Text = "Đã tạo cặp khóa RSA-2048 trong bộ nhớ" };
    private readonly Label _hashLabel = new() { AutoSize = true };

    public MainForm()
    {
        Text = "Phòng thí nghiệm RSA — PEM, SHA-256 và chữ ký số";
        Width = 1120; Height = 800; MinimumSize = new Size(920, 650);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildCryptoTab());
        tabs.TabPages.Add(BuildHashSignTab());
        tabs.TabPages.Add(BuildPemTab());
        Controls.Add(tabs);
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(10, 5, 10, 5) };
        footer.Controls.Add(_keyState);
        Controls.Add(footer);
        _hashInput.TextChanged += (_, _) => UpdateHash();
        _hashInput.Text = "Hello RSA";
    }

    private TabPage BuildCryptoTab()
    {
        var page = new TabPage("Mã hóa RSA");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 6 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        var keyPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        keyPanel.Controls.Add(Btn("Tạo khóa RSA-2048", (_, _) => Guard(GenerateKey)));
        keyPanel.Controls.Add(Btn("Xuất khóa công khai + khóa riêng PEM", (_, _) => Guard(ExportKeys)));
        keyPanel.Controls.Add(Btn("Nạp tệp PEM", (_, _) => Guard(ImportPem)));
        root.Controls.Add(keyPanel, 0, 0); root.SetColumnSpan(keyPanel, 2);
        root.Controls.Add(new Label { Text = "Văn bản gốc (UTF-8, tối đa 190 byte)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        root.Controls.Add(new Label { Text = "Dữ liệu đã mã hóa (Base64)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, 1);
        root.Controls.Add(_input, 0, 2);
        root.Controls.Add(_cipher, 1, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        actions.Controls.Add(Btn("Mã hóa bằng khóa công khai", (_, _) => Guard(Encrypt)));
        actions.Controls.Add(Btn("Giải mã bằng khóa riêng", (_, _) => Guard(Decrypt)));
        actions.Controls.Add(Btn("Nạp tệp Base64", (_, _) => Guard(LoadCipher)));
        actions.Controls.Add(Btn("Lưu Base64 ra tệp", (_, _) => Guard(SaveCipher)));
        root.Controls.Add(actions, 0, 3); root.SetColumnSpan(actions, 2);
        root.Controls.Add(new Label { Text = "OAEP với SHA-256: khóa RSA 2048 bit chỉ mã hóa tối đa 190 byte. RSA dùng cho dữ liệu nhỏ; hãy dùng AES cho tệp lớn.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
        root.SetColumnSpan(root.GetControlFromPosition(0, 4)!, 2);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill };
        bottom.Controls.Add(_byteCount);
        root.Controls.Add(bottom, 0, 5); root.SetColumnSpan(bottom, 2);
        page.Controls.Add(root);
        return page;
    }
    private TabPage BuildHashSignTab()
    {
        var page = new TabPage("SHA-256 và chữ ký số");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.Controls.Add(new Label { Text = "Dữ liệu đầu vào (sửa để quan sát hiệu ứng avalanche)", Dock = DockStyle.Fill }, 0, 0);
        root.Controls.Add(new Label { Text = "Giá trị băm SHA-256 (hex)", Dock = DockStyle.Fill }, 1, 0);
        root.Controls.Add(_hashInput, 0, 1); root.Controls.Add(_hash, 1, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        actions.Controls.Add(Btn("Ký bằng khóa riêng (RSA-PSS)", (_, _) => Guard(Sign)));
        actions.Controls.Add(Btn("Xác minh chữ ký", (_, _) => Guard(Verify)));
        actions.Controls.Add(Btn("Đo tốc độ 100 lần", (_, _) => Guard(Benchmark)));
        root.Controls.Add(actions, 0, 2); root.SetColumnSpan(actions, 2);
        root.Controls.Add(new Label { Text = "Chữ ký (Base64). RSA-PSS dùng salt ngẫu nhiên nên cùng dữ liệu và khóa riêng có thể tạo chữ ký khác nhau nhưng đều hợp lệ.", Dock = DockStyle.Fill }, 0, 3);
        root.SetColumnSpan(root.GetControlFromPosition(0, 3)!, 2);
        root.Controls.Add(_signature, 0, 4); root.SetColumnSpan(_signature, 2);
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildPemTab()
    {
        var page = new TabPage("Trình xem cấu trúc PEM");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        actions.Controls.Add(Btn("Mở PEM và xem các byte", (_, _) => Guard(ViewPem)));
        actions.Controls.Add(Btn("Hiển thị tham số khóa hiện tại", (_, _) => Guard(ViewParameters)));
        root.Controls.Add(actions, 0, 0); root.Controls.Add(_pemView, 0, 1);
        page.Controls.Add(root);
        return page;
    }

    private static Button Btn(string text, EventHandler action)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4) };
        b.Click += action; return b;
    }

    private void Guard(Action action)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Thao tác thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private void GenerateKey()
    {
        // Replace the active key by importing a newly generated private PEM.
        using var fresh = RSA.Create(2048);
        _rsa.ImportPem(fresh.ExportPkcs8PrivateKeyPem());
        _keyState.Text = "Đã tạo cặp khóa RSA-2048 mới trong bộ nhớ";
        MessageBox.Show(this, "Đã tạo cặp khóa RSA-2048 mới trong bộ nhớ. Hãy xuất ra PEM để lưu lại.", "RSA");
    }

    private void ExportKeys()
    {
        using var folder = new FolderBrowserDialog { Description = "Chọn thư mục lưu public.pem và private.pem" };
        if (folder.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(Path.Combine(folder.SelectedPath, "public.pem"), _rsa.ExportPublicPem(), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(folder.SelectedPath, "private.pem"), _rsa.ExportPrivatePem(), new UTF8Encoding(false));
        _keyState.Text = "PEM files exported to " + folder.SelectedPath;
        MessageBox.Show(this, "Đã lưu public.pem và private.pem. Hãy giữ bí mật tệp private.pem.", "Xuất PEM");
    }

    private void ImportPem()
    {
        using var dialog = new OpenFileDialog { Filter = "PEM files (*.pem)|*.pem|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string pem = File.ReadAllText(dialog.FileName);
        _rsa.ImportPem(pem);
        _keyState.Text = "Đã nạp PEM: " + Path.GetFileName(dialog.FileName);
        MessageBox.Show(this, "Đã nạp PEM. Khóa công khai dùng để mã hóa/xác minh; khóa riêng dùng để giải mã/ký.", "Nạp PEM");
    }

    private byte[] InputBytes()
    {
        byte[] data = Encoding.UTF8.GetBytes(_input.Text);
        if (data.Length > 190)
            throw new InvalidOperationException($"Input is {data.Length} UTF-8 bytes. The limit is 190 bytes for RSA-2048 OAEP-SHA256.");
        return data;
    }

    private void Encrypt()
    {
        byte[] data = InputBytes();
        _cipher.Text = Convert.ToBase64String(_rsa.Encrypt(data));
    }

    private void Decrypt()
    {
        byte[] encrypted = Convert.FromBase64String(_cipher.Text.Trim());
        _input.Text = Encoding.UTF8.GetString(_rsa.Decrypt(encrypted));
    }

    private void LoadCipher()
    {
        using var dialog = new OpenFileDialog { Filter = "Tệp Base64 (*.b64;*.txt)|*.b64;*.txt|Tất cả tệp (*.*)|*.*" };
        if (dialog.ShowDialog(this) == DialogResult.OK) _cipher.Text = File.ReadAllText(dialog.FileName).Trim();
    }

    private void SaveCipher()
    {
        using var dialog = new SaveFileDialog { Filter = "Base64 text (*.b64)|*.b64|Text file (*.txt)|*.txt", FileName = "rsa_cipher.b64" };
        if (dialog.ShowDialog(this) == DialogResult.OK) File.WriteAllText(dialog.FileName, _cipher.Text.Trim(), Encoding.ASCII);
    }

    private void UpdateHash()
    {
        byte[] data = Encoding.UTF8.GetBytes(_hashInput.Text);
        _hash.Text = Convert.ToHexString(SHA256.HashData(data));
        _byteCount.Text = $"Kích thước đầu vào: {data.Length} byte UTF-8 (giới hạn RSA: 190 byte)";
        _byteCount.ForeColor = data.Length > 190 ? Color.Firebrick : SystemColors.ControlText;
        _hashLabel.Text = "SHA-256 updates whenever input changes";
    }
    private void Sign()
    {
        byte[] data = Encoding.UTF8.GetBytes(_hashInput.Text);
        _signature.Text = Convert.ToBase64String(_rsa.Sign(data));
    }

    private void Verify()
    {
        byte[] data = Encoding.UTF8.GetBytes(_hashInput.Text);
        byte[] signature = Convert.FromBase64String(_signature.Text.Trim());
        bool valid = _rsa.Verify(data, signature);
        MessageBox.Show(this, valid ? "HỢP LỆ: chữ ký khớp với dữ liệu và khóa công khai." : "KHÔNG HỢP LỆ: dữ liệu, chữ ký hoặc khóa không khớp.", "Xác minh chữ ký",
            MessageBoxButtons.OK, valid ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void Benchmark()
    {
        byte[] data = Encoding.UTF8.GetBytes(_hashInput.Text);
        if (data.Length > 190)
            throw new InvalidOperationException("For the RSA-OAEP part, use input no longer than 190 UTF-8 bytes.");
        _pemView.Text = RsaBenchmarks.Run(_rsa, data);
        MessageBox.Show(this, "Kết quả đo tốc độ đã được ghi vào tab Trình xem PEM.", "Benchmark");
    }

    private void ViewPem()
    {
        using var dialog = new OpenFileDialog { Filter = "PEM files (*.pem)|*.pem|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string pem = File.ReadAllText(dialog.FileName);
        var sb = new StringBuilder();
        sb.AppendLine("FILE: " + dialog.FileName);
        sb.AppendLine("----- ORIGINAL PEM TEXT -----");
        sb.AppendLine(pem);
        sb.AppendLine("----- DECODED DER BYTES -----");
        sb.AppendLine(RsaService.PemBytesView(pem));
        try
        {
            using var temp = RSA.Create();
            temp.ImportFromPem(pem);
            RSAParameters parameters;
            bool hasPrivate;
            try { parameters = temp.ExportParameters(true); hasPrivate = parameters.D is not null; }
            catch { parameters = temp.ExportParameters(false); hasPrivate = false; }
            sb.AppendLine("----- PARSED RSA PARAMETERS -----");
            sb.AppendLine(RsaService.ParametersView(parameters, hasPrivate));
        }
        catch (Exception ex) { sb.AppendLine("Không thể phân tích tham số RSA: " + ex.Message); }
        _pemView.Text = sb.ToString();
    }

    private void ViewParameters()
    {
        RSAParameters p;
        bool hasPrivate;
        try { p = _rsa.ExportParameters(true); hasPrivate = p.D is not null; }
        catch { p = _rsa.ExportParameters(false); hasPrivate = false; }
        _pemView.Text = RsaService.ParametersView(p, hasPrivate) +
            "\r\nGhi chú định dạng PEM:\r\n" +
            "-----BEGIN PUBLIC KEY----- thường chứa SubjectPublicKeyInfo (định danh thuật toán + khóa công khai).\r\n" +
            "-----BEGIN PRIVATE KEY----- thường chứa cấu trúc khóa riêng PKCS#8.\r\n" +
            "Base64 là cách mã hóa văn bản, không phải mã hóa bảo mật. DER là định dạng nhị phân ASN.1.\r\n" +
            "Thành phần khóa công khai RSA: n và e. Khóa riêng có d và các giá trị CRT p, q, DP, DQ, InverseQ.";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _rsa.Dispose();
        base.Dispose(disposing);
    }
}

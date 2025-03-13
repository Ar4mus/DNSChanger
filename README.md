# DNS Changer

## 📌 Overview

DNS Changer is a **WPF application** built with **.NET 8** that allows users to manage and switch between multiple DNS configurations. It provides an easy-to-use interface to set **custom DNS servers**, revert to **automatic DNS settings**, and maintain a **local list of DNS configurations**.

## 🚀 Features

- **Select a DNS from a dropdown list** and apply it to the active network adapter.
- **Save and load DNS configurations** from a local JSON file.
- **Default DNS configurations** (Electro, RadarGame, Shekan, etc.) included.
- **Revert to automatic DNS settings** with a single click.
- **Delete saved DNS entries** (except for automatic mode).
- **Displays the current active DNS settings**.

## 🛠️ Installation

### **1️⃣ Prerequisites**

- Windows 10/11
- .NET 8 Runtime & SDK ([Download Here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- Administrator privileges (Required to modify DNS settings)

### **2️⃣ Download the Published Version**

You can download the **pre-built release** from the [GitHub Releases](https://github.com/Ar4mus/DNSChanger/releases/tag/Latest). 

After downloading, ensure that you have **.NET 8 installed** to run the application.

### **3️⃣ Clone the Repository (For Developers)**

```sh
 git clone https://github.com/yourusername/DNSChanger.git
 cd DNSChanger
```

### **4️⃣ Build and Run (For Developers)**

```sh
 dotnet build
 dotnet run --project DNSChanger
```

## 🎯 How to Use

1. **Select a DNS configuration** from the dropdown list.
2. Click **"Set DNS"** to apply the selected DNS.
3. Click **"Delete"** to remove a saved DNS entry (except Automatic DNS).
4. Click **"Add New DNS"** to save a new configuration.
5. The **current DNS settings** will be displayed at the top.

## 📂 Project Structure

```
📁 DNSChanger
│── 📄 DNSChanger.csproj         # Main project file
│── 📄 App.xaml                  # WPF App entry point
│── 📄 MainWindow.xaml           # UI layout
│── 📄 MainWindow.xaml.cs        # Main logic & DNS management
│── 📄 FileHandler.cs            # Handles local JSON storage
│── 📄 DnsHelper.cs              # Utility functions for setting DNS
│── 📄 DnsEntry.cs               # Data model for DNS configurations
│── 📄 dns_settings.json         # Local storage for DNS configurations
```

## ✅ Default DNS Configurations

These DNS servers are included in `dns_settings.json` and will be automatically created on first run:

| Name           | Primary DNS    | Secondary DNS  |
| -------------- | -------------- | -------------- |
| Electro        | 78.157.42.100  | 78.157.42.101  |
| RadarGame      | 10.202.10.10   | 10.202.10.11   |
| Shekan         | 178.22.122.100 | 185.51.200.2   |
| Begzar         | 185.55.226.26  | 185.55.225.25  |
| 403DNS         | 10.202.10.202  | 10.202.10.102  |
| Beshkan        | 181.41.194.177 | 181.41.194.186 |
| Google DNS     | 8.8.8.8        | 8.8.4.4        |
| Cloudflare DNS | 1.1.1.1        | 1.0.0.1        |
| OpenDNS        | 208.67.222.222 | 208.67.220.220 |

## 🔒 Permissions & Security

- **Requires Administrator Privileges** to change system DNS settings.
- Uses `netsh` commands internally to modify DNS settings.

## 📜 License

This project is licensed under the **MIT License**. Feel free to use and modify it as needed.

## 🤝 Contributing

Pull requests are welcome! If you find a bug or want to add a feature, please open an issue first.

## 📧 Contact
For questions or issues, reach out via **[Sattar.bayat@gmail.com](mailto:Sattar.bayat@gmail.com)** or create an issue on **GitHub**.


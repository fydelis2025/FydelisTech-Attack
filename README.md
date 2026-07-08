# FydelisTech-ATTACK 🛡️ Autor: Adiel Santos Fontes🚀

![Status do Projeto](https://img.shields.io/badge/Status-Desenvolvimento-brightgreen)
![Linguagem](https://img.shields.io/badge/Language-.NET%20%2F%20C%23-blue)
![Versão](https://img.shields.io/badge/Version-v1.0.0-green)
![Plataforma](https://img.shields.io/badge/Platform-Windows-lightgrey)

O **FydelisTech-ATTACK** é uma suíte moderna e de alta performance voltada para Reconhecimento OSINT (Open Source Intelligence) e análise de superfície de ataque (*Attack Surface Management*). Desenvolvido inteiramente em **C#**, o sistema une uma interface gráfica intuitiva e robusta com ferramentas avançadas de varredura automatizada para auxiliar profissionais de segurança e PENTESTERs na identificação de ativos expostos na web.

---

## 📸 Interface do Sistema

<p align="center">
  <img src="https://github.com/fydelis2025/FydelisTech-Attack/blob/main/Captura de tela 2026-07-08 112229.png" alt="FydelisTech-ATTACK Dashboard" width="100%">
</p>
<p align="center">
  <img src="https://github.com/fydelis2025/FydelisTech-Attack/blob/main/Captura de tela 2026-07-08 112246.png" alt="FydelisTech-ATTACK Dashboard" width="100%">
</p>
<p align="center">
  <img src="https://github.com/fydelis2025/FydelisTech-Attack/blob/main/Captura de tela 2026-07-08 112301.png" alt="FydelisTech-ATTACK Dashboard" width="100%">
</p>
<p align="center">
  <img src="https://github.com/fydelis2025/FydelisTech-Attack/blob/main/Captura de tela 2026-07-08 112311.png" alt="FydelisTech-ATTACK Dashboard" width="100%">
</p>
<p align="center">
  <img src="https://github.com/fydelis2025/FydelisTech-Attack/blob/main/Captura de tela 2026-07-08 112321.png" alt="FydelisTech-ATTACK Dashboard" width="100%">
</p>
## ⚠️ Termos de Uso e Propriedade Intelectual

* **Uso Moderado e Ético:** Esta ferramenta foi desenvolvida exclusivamente para fins educacionais, testes de intrusão autorizados e mapeamento defensivo de ativos (OSINT). Utilize com moderação e estrita responsabilidade. O uso abusivo ou sem autorização expressa dos proprietários dos alvos pode violar leis locais de segurança cibernética.
* **Proteção de Direitos Autorais:** O design da interface gráfica, a identidade visual da suíte **FydelisTech-ATTACK** e a arquitetura lógica do sistema são de propriedade intelectual exclusiva de **Adiel Fontes**. É expressamente proibida a redistribuição, engenharia reversa, sublicenciamento ou comercialização não autorizada deste software ou de suas partes sem consentimento prévio por escrito.
---

## ✨ Funcionalidades Principais (v1.0.0)

* 🔍 **DNS Enumeration:** Mapeamento completo e estruturado de registros de DNS para o domínio alvo.
* 🌐 **Subdomain Scan:** Algoritmo de varredura rápida para descoberta de subdomínios associados.
* ⚡ **HTTP Probe:** Verificação em tempo real do status de hosts vivos e identificação de serviços HTTP/HTTPS ativos.
* 🛡️ **WAF Detection:** Identificação inteligente de Web Application Firewalls (como Cloudflare) protegendo o alvo.
* 📊 **Console de Logs:** Monitoramento detalhado das fases do scan em tempo real com carimbo de data/hora (*timestamp*).
* 📥 **Exportação Inteligente:** Opções nativas para salvar e exportar os relatórios gerados nos formatos **JSON** e **HTML** para documentação de auditorias.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# (.NET Framework / .NET Core)
* **Arquitetura/UI:** Design Cyberpunk moderno focado em alta usabilidade e responsividade.
* **Gerenciamento de Threads:** Processamento concorrente para execução de testes de rede sem travamento da interface gráfica.

---

## 🚀 Como Executar o Projeto

### Pré-requisitos
* Sistema Operacional Windows.
* [.NET Runtime / SDK](https://dotnet.microsoft.com/download) correspondente instalado.

### Executando via Binário (Recomendado)
1. Vá até a aba [Releases](https://github.com/fydelis2025/FydelisTech-Attack/releases) no menu lateral direito.
2. Baixe a versão mais recente compilada (`.zip`).
3. Extraia os arquivos e execute o `FydelisTech-ATTACK.exe`.

### Compilando o Código Fonte
Se preferir compilar manualmente utilizando o Visual Studio ou CLI do .NET:
```bash
# Clone o repositório
git clone [https://github.com/fydelis2025/FydelisTech-Attack.git](https://github.com/fydelis2025/FydelisTech-Attack.git)

# Acesse a pasta do projeto
cd FydelisTech-Attack

# Restaure as dependências e compile o projeto
dotnet build

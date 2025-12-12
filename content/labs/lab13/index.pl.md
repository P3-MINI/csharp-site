---
title: "Lab13"
weight: 10
---

# Laboratorium 13: Strumienie sieciowe, Łącza, Mapowanie plików

## Kod startowy

Kod startowy dostępny jest w formie repozytorium `P3Z-25Z-Task6` na GitHubie w organizacji WUT-MiNI.

{{< tabs >}}
{{% tab "SSH" %}} 
```bash
# Clone the repository:
git clone git@github.com:WUT-MiNI/P3Z-25Z-Task6.git

# Change the origin url:
git remote set-url origin $(Your-personal-repository-address-for-this-task)
```
{{% /tab %}}
{{% tab "HTTPS" %}} 
```bash
# Clone the repository:
git clone https://github.com/WUT-MiNI/P3Z-25Z-Task6.git

# Change the origin url:
git remote set-url origin $(Your-personal-repository-address-for-this-task)
```
{{% /tab %}}
{{< /tabs >}}

## Fairplay

W repozytorium znajduje się skrypt monitorujący pracę. Należy go uruchomić. Na Linuxie będzie to `fairplay.desktop`; na Windowsie `fairplay.bat`. Skrypt uruchamia program nagrywający ekran. Nagranie jest zapisywane na dysku sieciowym. Kod źródłowy programu do wglądu na repozytorium [csharp-rec](https://github.com/P3-MINI/csharp-rec/tree/master).

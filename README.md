# Programming 3 – Advanced Course Website

This repository contains the source code for the website of the “Programming 3 – Advanced” course offered by the Faculty of Mathematics and Information Science, Warsaw University of Technology (MiNI PW).

Website URL: https://csharp.mini.pw.edu.pl
GitHub Pages Mirror: https://p3-mini.github.io/csharp-site

The site is built using [Hugo](https://gohugo.io/), a static site generator. It includes [hugo-book](https://themes.gohugo.io/themes/hugo-book/) theme added as a git submodule. The site is automatically deployed on every push to the master branch.

## Cloning the Repository

This repository uses git submodules for its theme. To properly clone the repository with submodules, use the `--recurse-submodules` option:

```bash
git clone --recurse-submodules https://github.com/P3-MINI/csharp-site.git
```

If you've already cloned the repository without `--recurse-submodules`, you can initialize the submodules manually:

```bash
git submodule update --init --recursive
```

## Running Locally

To run the website locally:

1. Navigate to the repository directory:
   ```bash
   cd csharp-site
   ```

2. Start the Hugo server:
   ```bash
   hugo server
   ```

3. Open your browser and go to:
   ```
   http://localhost:1313/
   ```

Changes will be reflected automatically in the browser as you modify the content.

## Contributing

If you're interested in contributing, please read the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines and instructions.

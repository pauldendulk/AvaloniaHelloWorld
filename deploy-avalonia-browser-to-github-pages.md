> **Agent Instructions: Deploy Avalonia Browser (WASM) to GitHub Pages (GitHub Actions)**
>
> **Goal**
> Deploy an Avalonia Browser app (`netX.0-browser`) to GitHub Pages using GitHub Actions, so the deployed result is a public web page.
>
> **Critical rule (prevents NETSDK1178)**
> Do NOT restore/build the whole solution if it includes Android/iOS/Desktop projects. On `ubuntu-latest`, that often triggers workload pack errors (NETSDK1178). Restore/publish ONLY the Browser project `.csproj`.
>
> ---
>
> ## 1) Verify project prerequisites
> 1. Confirm there is a Browser project, e.g. `MyApp.Browser/MyApp.Browser.csproj`.
> 2. Confirm the Browser project uses WebAssembly SDK and Avalonia.Browser (typical):
>    - `<Project Sdk="Microsoft.NET.Sdk.WebAssembly">`
>    - `<TargetFramework>net9.0-browser</TargetFramework>` (or net8/net10 as appropriate)
>    - `PackageReference` includes `Avalonia.Browser`
> 3. Confirm local publish produces static site assets:
>    - Run: dotnet publish ./MyApp.Browser/MyApp.Browser.csproj -c Release -o ./publish
>    - Verify: ./publish/wwwroot/index.html exists
>
> ---
>
> ## 2) Pin the .NET SDK used by CI (recommended)
> Add a `global.json` at the repository root (or the folder the workflow builds from) to force the same SDK on GitHub Actions.
>
> Example (pin to .NET 9 feature band):
> {
>   "sdk": {
>     "version": "9.0.100",
>     "rollForward": "latestFeature",
>     "allowPrerelease": false
>   }
> }
>
> Notes:
> - This prevents the runner from unexpectedly using .NET 10/8 if they’re preinstalled.
> - If your project targets net8.0-browser or net10.0-browser, pin accordingly.
>
> ---
>
> ## 3) Create the GitHub Pages workflow
> Create file: .github/workflows/deploy.yml
>
> IMPORTANT: Replace `MyApp.Browser/MyApp.Browser.csproj` with your real Browser project path.
>
> name: Deploy (Browser) to GitHub Pages
>
> on:
>   push:
>     branches: [main, master]
>   workflow_dispatch:
>
> permissions:
>   contents: read
>   pages: write
>   id-token: write
>
> concurrency:
>   group: pages
>   cancel-in-progress: false
>
> jobs:
>   build:
>     runs-on: ubuntu-latest
>     steps:
>       - name: Checkout
>         uses: actions/checkout@v4
>
>       # Strongly recommended: honor global.json so the SDK is deterministic.
>       - name: Setup .NET (global.json)
>         uses: actions/setup-dotnet@v4
>         with:
>           global-json-file: ./global.json
>
>       - name: Verify .NET
>         run: |
>           dotnet --version
>           dotnet --info
>
>       # Avalonia Browser needs the WASM workload on the runner.
>       - name: Install WASM workload
>         run: dotnet workload install wasm-tools
>
>       # Critical: restore ONLY the Browser project (avoid Android/iOS workload requirements).
>       - name: Restore (Browser only)
>         run: dotnet restore ./MyApp.Browser/MyApp.Browser.csproj
>
>       - name: Publish (Browser)
>         run: dotnet publish ./MyApp.Browser/MyApp.Browser.csproj -c Release -o ./publish
>
>       - name: Configure Pages
>         uses: actions/configure-pages@v5
>
>       - name: Upload Pages artifact
>         uses: actions/upload-pages-artifact@v3
>         with:
>           path: publish/wwwroot
>
>   deploy:
>     runs-on: ubuntu-latest
>     needs: build
>     environment:
>       name: github-pages
>       url: ${{ steps.deployment.outputs.page_url }}
>     steps:
>       - name: Deploy to GitHub Pages
>         id: deployment
>         uses: actions/deploy-pages@v4
>
> ---
>
> ## 4) Configure GitHub Pages to use GitHub Actions
> In the GitHub repo:
> - Settings → Pages → Build and deployment
> - Source: GitHub Actions
>
> Then either:
> - Push to `main` or `master`, or
> - Run the workflow manually via Actions → the workflow → Run workflow
>
> ---
>
> ## 5) Troubleshooting (fast checklist)
> A) NETSDK1178 “workload packs do not exist”
> - Make sure the workflow restores/publishes ONLY the Browser `.csproj` (not the `.sln`).
> - Ensure `dotnet workload install wasm-tools` runs before restore/publish.
>
> B) Runner uses wrong SDK (log shows 10.x when you expect 9.x)
> - Use setup-dotnet with `global-json-file`.
> - Ensure `global.json` is in the location referenced.
> - Keep the `dotnet --info` step so the log proves what it used.
>
> C) Deployed site is blank/404
> - Verify artifact path is `publish/wwwroot`.
> - Confirm `index.html` is inside that folder after publish.
>
> D) Works locally but not in Actions
> - Compare local `dotnet --info` to the workflow’s `dotnet --info`.
> - If needed, pin to a more specific SDK (e.g., 9.0.307) in global.json.
>
> ---
>
> **Deliverable**
> After a successful run, GitHub will show the deployed URL in the workflow summary, and the Pages environment will expose the final page URL.

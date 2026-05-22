# GitHub publish guide

## 1. Create a repository on GitHub

Recommended repository name:

```text
FuSheng.AnimationKit.WinForms
```

Description:

```text
Lightweight WinForms animation library providing luminous shine effects and fragment-based page transitions.
```

Recommended settings:

- Visibility: Public
- Add README: no, this project already has one
- Add .gitignore: no, this project already has one
- Add license: no, this project already has MIT license

## 2. Upload by Git command

Open terminal in this folder and run:

```bash
git init
git add .
git commit -m "Initial open-source release"
git branch -M main
git remote add origin https://github.com/YOUR_NAME/FuSheng.AnimationKit.WinForms.git
git push -u origin main
```

Replace `YOUR_NAME` with your GitHub username.

## 3. Release version suggestion

First public version:

```text
v0.1.0-alpha
```

Do not call it v1.0 yet. This is an API extraction preview.

## 4. What should not be added

Do not add these to this animation library:

- FuShengSDK product registry paths
- NV FS executable logic
- QQ group links
- download links
- private icons
- business UI text unrelated to animation

This repository should only contain reusable animation code.

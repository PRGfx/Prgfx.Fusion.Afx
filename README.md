# Prgfx.Fusion.Afx
A C# port of the [Neos.Fusion.Afx](https://github.com/neos/fusion-afx) Fusion DSL for the [Neos CMS](https://neos.io) - a "JSX inspired compact syntax for Neos.Fusion".

To be used with [Prgfx.Fusion](https://github.com/PRGfx/Prgfx.Fusion):

```c#
var parser = new Prgfx.Fusion.Parser();
var dslFactory = new Prgfx.Fusion.DslFactory();
dslFactory.RegisterDsl("afx", new Prgfx.Fusion.Afx.AfxDsl());
parser.SetDslFactory(dslFactory);
// ...
```
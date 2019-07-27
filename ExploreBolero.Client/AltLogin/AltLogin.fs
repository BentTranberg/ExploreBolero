namespace ExploreBolero.Client.AltLogin

module View =

    open Bolero

    type Tmpl = Template<"AltLogin/altlogin.html">

    let altLoginPage () = Tmpl().Elt()

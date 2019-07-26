namespace BoleroApp.Client.Home

module View =

    open Bolero

    type Tmpl = Template<"Home/home.html">

    let homePage () = Tmpl().Elt()

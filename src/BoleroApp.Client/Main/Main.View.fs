module BoleroApp.Client.Main.View

open BoleroApp.Client
open Bolero
open Bolero.Html
open Elmish

type Tmpl = Template<"Main/main.html">

let menuItem (model: Model) (page: Page) (text: string) =
    Tmpl.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(Router.router.Link page)
        .Text(text)
        .Elt()

let homePage () =
    Tmpl.Home().Elt()

let view (model: Model) (dispatch: Dispatch<Message>) =
    Tmpl()
        .Menu(concat [
            menuItem model Page.Home "Home"
            menuItem model Page.Counter "Counter"
            menuItem model Page.Data "Download data"
            Login.View.logoutButton model.login (dispatch << Message.Login)
        ])
        .Body(
            cond model.page <| function
            | Page.Home -> homePage ()
            | Page.Counter -> Counter.View.page model.counter (dispatch << Message.Counter)
            | Page.Data ->
                cond model.login.signedInAs <| function
                | Some _ -> Books.View.page model.books (dispatch << Message.Books)
                | None -> Login.View.page model.login (dispatch << Message.Login)
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Common.View.Tmpl.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

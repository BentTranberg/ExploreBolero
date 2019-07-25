module BoleroApp.Client.Login.View

open Bolero
open Bolero.Html
open BoleroApp.Client
open Elmish

type Tmpl = Template<"Login/login.html">

let page (model: Model) (dispatch: Dispatch<Message>) =
    Tmpl()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty
            | true ->
                Common.View.Tmpl.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()

let logoutButton (model: Model) (dispatch: Dispatch<Message>) =
    cond model.signedInAs <| function
    | None -> empty
    | Some loggedInAs ->
        Tmpl.LogoutButton()
            .SignOut(fun _ -> dispatch SendSignOut)
            .Username(loggedInAs)
            .Elt()

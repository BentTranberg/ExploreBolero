module BoleroApp.Client.Books.Update

open Bolero.Remoting.Client
open Elmish

let update (remote: RemoteService) (message: Message) (model: Model) =
    match message with
    | GetBooks ->
        { model with books = None }, Cmd.ofAsync remote.getBooks () GotBooks Error
    | GotBooks books ->
        { model with books = Some books }, Cmd.none
    | Error exn ->
        model, Cmd.none

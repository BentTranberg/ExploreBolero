module BoleroApp.Client.Books.View

open Bolero
open Bolero.Html
open Elmish

type Tmpl = Template<"Books/books.html">

let page (model: Model) (dispatch: Dispatch<Message>) =
    Tmpl()
        .Reload(fun _ -> dispatch GetBooks)
        .Rows(cond model.books <| function
            | None ->
                Tmpl.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr [] [
                        td [] [text book.title]
                        td [] [text book.author]
                        td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
                        td [] [text book.isbn]
                    ])
        .Elt()

namespace ExploreBolero.Client.Books

open System
open Bolero.Json

type Book =
    {
        title: string
        author: string
        [<DateTimeFormat "yyyy-MM-dd">] publishDate: DateTime
        isbn: string
    }

type RemoteService =
    {
        getBooks: unit -> Async<Book[]>
        addBook: Book -> Async<unit>
        removeBookByIsbn: string -> Async<unit>
    }

    interface Bolero.Remoting.IRemoteService with
        member this.BasePath = "/books"

type Message =
    | GetBooks
    | GotBooks of Book[]
    | Error of exn

type Model =
    {
        books: Book[] option
    }

module Model =

    open Bolero.Remoting.Client
    open Elmish

    let init = { books = None }

    let update (remote: RemoteService) (message: Message) (model: Model) =
        match message with
        | GetBooks ->
            { model with books = None }, Cmd.ofAsync remote.getBooks () GotBooks Error
        | GotBooks books ->
            { model with books = Some books }, Cmd.none
        | Error exn ->
            model, Cmd.none

module View =

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

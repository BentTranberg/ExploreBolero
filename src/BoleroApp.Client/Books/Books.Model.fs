namespace BoleroApp.Client.Books

open Bolero.Json
open System
open Bolero.Remoting

type Book =
    {
        title: string
        author: string
        [<DateTimeFormat "yyyy-MM-dd">]
        publishDate: DateTime
        isbn: string
    }

type Model =
    {
        books: Book[] option
    }

module Model =

    let init = { books = None }

type Message =
    | GetBooks
    | GotBooks of Book[]
    | Error of exn

/// Remote service definition.
type RemoteService =
    {
        /// Get the list of all books in the collection.
        getBooks: unit -> Async<Book[]>

        /// Add a book in the collection.
        addBook: Book -> Async<unit>

        /// Remove a book from the collection, identified by its ISBN.
        removeBookByIsbn: string -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

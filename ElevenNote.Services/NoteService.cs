﻿using ElevenNote.Data;
using ElevenNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElevenNote.Services
{
    public class NoteService
    {
        private readonly Guid _userId;
        public NoteService(Guid userId)
        {
            _userId = userId;
        }

        public bool CreateNote(NoteCreate model)
        {
            var entity =
                new Note()
                {
                    OwnerId = _userId,
                    Title = model.Title,
                    Content = model.Content,
                    CreatedUtc = DateTimeOffset.Now
                };

            using (var ctx = new ApplicationDbContext())
            {
                ctx.Notes.Add(entity);
                return ctx.SaveChanges() == 1;
            }
        }

        public IEnumerable<NoteListItem> GetNotes()    // Getting all notes from ONE user and putting them into a list
        {                                              // Creates a temporary instance of ApplicationDbContext
            using (var ctx = new ApplicationDbContext())
            {                                          // Instantiates an IQueryable called query
                var query =
                    ctx                                // <--- our ApplicationDbContext object from line 40
                        .Notes                         // <--- Notes DbSet<Note>
                        .Where(e => e.OwnerId == _userId)   // <--- Filters through Notes for entities with an OwnerId that matches the current User's Id
                        .Select(                       // <--- Iterates through the entities that passed through the filter, and performs whatever code you put inside its body
                        e =>                           // <--- Uses a lamda to take whatever entity the .Select method is currently running code for (e is our Note entity)
                            new NoteListItem           // <--- Creates a new NoteListItem to essentially "copy" the properties from e (our Note) to the NoteListItem
                            {
                                NoteId = e.NoteId,
                                Title = e.Title,
                                CreatedUtc = e.CreatedUtc
                            }                          // <--- This NoteListItem is added to our IQueryable object query
                       );
                return query.ToArray();                // <--- Converts our Iqueryable object to an Array
            }
        }
        public NoteDetail GetNoteById(int id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                        .Notes
                        .Single(e => e.NoteId == id && e.OwnerId == _userId);
                    return
                        new NoteDetail
                        {
                            NoteId = entity.NoteId,
                            Title = entity.Title,
                            Content = entity.Content,
                            CreatedUtc = entity.CreatedUtc,
                            ModifiedUtc = entity.ModifiedUtc
                        };
            }
        }

        public bool UpdateNote(NoteEdit model)
        {
            using(var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                        .Notes
                        .Single(e => e.NoteId == model.NoteId && e.OwnerId == _userId);

                entity.Title = model.Title;
                entity.Content = model.Content;
                entity.ModifiedUtc = DateTimeOffset.UtcNow;

                return ctx.SaveChanges() == 1;
            }
        }
        public bool DeleteNote(int noteId)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                        .Notes
                        .Single(e => e.NoteId == noteId && e.OwnerId == _userId);

                ctx.Notes.Remove(entity);

                return ctx.SaveChanges() == 1;
            }
        }
    }
}

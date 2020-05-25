using System;
using System.Threading.Tasks;
using Amver.EfCli;
using FillObjectionableReasonTable.Constants;

namespace FillObjectionableReasonTable
{
    public class Filler : IFiller
    {
        private readonly IContextFactory<ApplicationContext> _context;

        public Filler(IContextFactory<ApplicationContext> context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Fill()
        {
            using (var context = _context.CreateContext())
            {
                foreach (var objectionableReason in ObjectionableReason.ObjectionableReasons)
                {
                    await context.ObjectionableReasons.AddAsync(new Amver.Domain.Entities.ObjectionableReason
                    {
                        Reason = objectionableReason.Value
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
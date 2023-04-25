using Application.Interfaces.Repository;
using DataManagement.Models;
using WebScraper.Models;

namespace DataManagement.Repositories
{
    public class FlatRepository : IFlatRepository
    {
        private readonly RentFinderDbContext _context;

        public FlatRepository(RentFinderDbContext context)
        {
            _context = context;
        }

        public void CreateFlats(List<FlatInfoModel> flatInfoModels)
        {
            foreach (var flat in flatInfoModels)
            {
                CreateFlat(flat);
            }
        }

        private void CreateFlat(FlatInfoModel flatInfoModel)
        {

            var flatDto = new FlatInfoDto()
            {
                Id = default,
                Title = flatInfoModel.Title,
                Cost = flatInfoModel.Cost,
                Description = flatInfoModel.Description,
                ViewsOnSite = flatInfoModel.ViewsOnSite,
                FlatPhoneId = 1
            };

            _context.FlatInfosDto.Add(flatDto);
            _context.SaveChanges();

            CreateOrUpgradePhoneNumber(flatInfoModel.PhoneNumber);

            var phoneId = _context.FlatPhonesDto.Single(p => p.Number == flatInfoModel.PhoneNumber).Id;

            //var flatId = flatDto.Id;

            //CreateFlatImages(flatInfoModel.LinksOfImages, flatId);
            
           // _context.FlatDateInfosDto.Add(new FlatDateInfoDto()
               // { FlatInfoId = flatId, SitePublication = flatInfoModel.SitePublication });
            //_context.SaveChanges();

        }



        private void CreateFlatImages(List<string> imageLinks, long flatId)
        {
            imageLinks.ForEach(link =>
            {
                _context.FlatLinkImages.Add(new FlatLinkImage() { FlatInfoId = flatId, Link = link });
                _context.SaveChanges();
            });
        }

        private void CreateOrUpgradePhoneNumber(string number)
        {
            var phoneModel = _context.FlatPhonesDto.SingleOrDefault(p => p.Number == number);

            if (phoneModel == null)
            {
                var flatPhone = new FlatPhoneDto() { Number = number, NumberMentionsOnSite = 1 };
                _context.FlatPhonesDto.Add(flatPhone);
                _context.SaveChanges();
            }
            else
            {
                phoneModel.NumberMentionsOnSite += 1;
                _context.FlatPhonesDto.Update(phoneModel);
                _context.SaveChanges();
            }
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Application.Interfaces.Repository;
using Application.Models;
using DataManagement.Converters;
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

        public void CreateFlats(List<FlatInfoModel> flats)
        {
            flats.ForEach(CreateFlat);
        }

        private void CreateFlat(FlatInfoModel flat)
        {
            var flatModelDto = _context.FlatInfosDto.FirstOrDefault(f => f.PageLink == flat.PageLink);

            if (flatModelDto != null)
            {
                if (flatModelDto.Cost > flat.Cost + 30)
                {
                    var flatDateDto = _context.FlatDateInfosDto.First(d => d.FlatInfoId == flatModelDto.Id);
                    flatDateDto.RefusePublication = null;
                    flatDateDto.TelegramPublication = null;
                    _context.FlatDateInfosDto.Update(flatDateDto);
                    _context.SaveChanges();

                    if (flatModelDto.Description != null && !flatModelDto.Description.Contains("The price has decreased"))
                    {
                        flatModelDto.Description = $"(The price has decreased by {flatModelDto.Cost - flat.Cost} $)"
                                                + flatModelDto.Description;
                    }
                    flatModelDto.Cost = flat.Cost;
                    _context.FlatInfosDto.Update(flatModelDto);
                    _context.SaveChanges();
                }

                return;
            }

            var phoneId = CreateOrUpgradePhoneNumberAndGetHisId(flat.PhoneNumber, flat.PageLink);

            var flatInfoId = CreateFlatInfoAndGetHisId(flat, phoneId);

            CreateFlatImages(flat.LinksOfImages, flatInfoId);

            if (flat.FlatCoordinateModel.Latitude != 0 && flat.FlatCoordinateModel.Longitude != 0) CreateFlatCoordinate(flat, flatInfoId);

            _context.FlatDateInfosDto.Add(new FlatDateInfoDto()
                { FlatInfoId = flatInfoId, SitePublication = flat.SitePublication });
            _context.SaveChanges();
        }

        private void CreateFlatCoordinate(FlatInfoModel flat, long flatId)
        {
            var flatCoordinateDto = new FlatCoordinateDto() { FlatInfoId = flatId, Latitude = flat.FlatCoordinateModel.Latitude, Longitude = flat.FlatCoordinateModel.Longitude };

            _context.FlatCoordinatesDto.Add(flatCoordinateDto);
            _context.SaveChanges();
        }
        
        private long CreateFlatInfoAndGetHisId(FlatInfoModel flatInfoModel, long phoneId)
        {
            var flatDto = new FlatInfoDto()
            {
                Title = flatInfoModel.Title,
                Cost = flatInfoModel.Cost,
                Description = flatInfoModel.Description,
                ViewsOnSite = flatInfoModel.ViewsOnSite,
                PageLink = flatInfoModel.PageLink,
                FlatPhoneId = phoneId,
                AdditionalInformation = flatInfoModel.ComfortStuffModel.ToJson()
            };

            _context.FlatInfosDto.Add(flatDto);
            _context.SaveChanges();

            return flatDto.Id;
        }

        private void CreateFlatImages(List<string> imageLinks, long flatId)
        {
            imageLinks.ForEach(link =>
            {
                _context.FlatLinksImage.Add(new FlatLinkImage() { FlatInfoId = flatId, Link = link });
                _context.SaveChanges();
            });
        }

        private long CreateOrUpgradePhoneNumberAndGetHisId(string number, string flatLink)
        {
            var phoneModel = _context.FlatPhonesDto.SingleOrDefault(p => p.Number == number);

            if (phoneModel == null)
            {
                var flatPhone = new FlatPhoneDto() { Number = number, NumberMentionsOnSite = 1 };
                _context.FlatPhonesDto.Add(flatPhone);
                _context.SaveChanges();
                return flatPhone.Id;
            }

            if (_context.FlatInfosDto.FirstOrDefault(f => flatLink == f.PageLink) == null)
            {
                phoneModel.NumberMentionsOnSite += 1;
                _context.FlatPhonesDto.Update(phoneModel);
                _context.SaveChanges();
            }

            return phoneModel.Id;
        }

        public long ReadCountNotViewedFlats()
        {
            return _context.FlatDateInfosDto.Count(f => f.RefusePublication == null && f.TelegramPublication == null);
        }
        
        public FlatInfoClientModel ReadOldestNotViewedFlat()
        {
            var noViewedFlatDateInfoDto = _context.FlatDateInfosDto
                .Where(d => d.RefusePublication == null && d.TelegramPublication == null)
                .OrderByDescending(x => x.SitePublication)
                .LastOrDefault();

            if (noViewedFlatDateInfoDto == null) return null;

            return ReadFlatById(noViewedFlatDateInfoDto.FlatInfoId);
        }

        public FlatInfoClientModel ReadFlatById(long flatId)
        {
            var flatModelDto = _context.FlatInfosDto.First(f => f.Id == flatId);

            var flatDateInfoDto = _context.FlatDateInfosDto.First(d => d.FlatInfoId == flatId);

            var flatPhoneModelDto = _context.FlatPhonesDto.First(p => p.Id == flatModelDto.FlatPhoneId);

            return new FlatInfoClientModel
            {
                Id = flatModelDto.Id,
                Title = flatModelDto.Title,
                Cost = flatModelDto.Cost,
                SitePublication = flatDateInfoDto.SitePublication,
                Description = flatModelDto.Description,
                FlatPhoneClientModel = new FlatPhoneClientModel() { PhoneNumber = flatPhoneModelDto.Number, MentionOnSite = flatPhoneModelDto.NumberMentionsOnSite },
                LinksOfImages = ReadFirstTenFlatImagesById(flatModelDto.Id),
                PageLink = flatModelDto.PageLink,
                ViewsOnSite = flatModelDto.ViewsOnSite.GetValueOrDefault(),
                FlatCoordinateClientModel = ReadFlatCoordinateOrGetDefaultById(flatModelDto.Id),
                ComfortStuffClientModel = flatModelDto.AdditionalInformation?.ToComfortStuffClientModel()
            };
        }

        public IEnumerable<(string phoneNumber, long cost)> ReadLastHourPostedFlats(DateTime time)
        {
            var phoneNumbers = _context.FlatPhonesDto;
            var middle = _context.FlatDateInfosDto
                .Where(d => d.TelegramPublication != null && d.TelegramPublication > time)
                .Select(d => d.FlatInfoId)
                .Join(_context.FlatInfosDto, id => id, up => up.Id, (id, up) => up)
                .ToList();

            return middle
                .Select(up => (phoneNumbers.First(x => x.Id == up.FlatPhoneId).Number, up.Cost))
                .ToList();
        }

        private FlatCoordinateClientModel ReadFlatCoordinateOrGetDefaultById(long flatId)
        {
            var flatCoordinateDto = _context.FlatCoordinatesDto.FirstOrDefault(c => c.FlatInfoId == flatId);

            if (flatCoordinateDto == null || flatCoordinateDto.Latitude == null || flatCoordinateDto.Longitude == null)
            {
                return new FlatCoordinateClientModel() { Latitude = 0, Longitude = 0 };
            }

            return new FlatCoordinateClientModel() { Latitude = flatCoordinateDto.Latitude.GetValueOrDefault(), Longitude = flatCoordinateDto.Longitude.GetValueOrDefault() };
        }

        private List<string> ReadFirstTenFlatImagesById(long flatId)
        {
            return _context.FlatLinksImage
                .Where(l => l.FlatInfoId == flatId)
                .Select(i => i.Link).Take(10).ToList();
        }

        public void UpdateFlatDateInfoTelegramPublication(long flatId, DateTime timeOfPublic)
        {
            var flatDateInfoDto = _context.FlatDateInfosDto.First(d => d.FlatInfoId == flatId);
            flatDateInfoDto.TelegramPublication = timeOfPublic;
            _context.SaveChanges();
        }

        public void UpdateFlatDateInfoRefusePublication(long flatId, DateTime timeOfRefuse)
        {
            var flatDateInfoDto = _context.FlatDateInfosDto.First(d => d.FlatInfoId == flatId);
            flatDateInfoDto.RefusePublication = timeOfRefuse;
            _context.SaveChanges();
        }

        public void UpdateFlatDateInfoTelegramException(long flatId, DateTime time)
        {
            var flatDateInfoDto = _context.FlatDateInfosDto.First(d => d.FlatInfoId == flatId);
            flatDateInfoDto.RefusePublication = time;
            flatDateInfoDto.TelegramPublication = time;
            _context.SaveChanges();
        }

        public void UpdatePhoneNumberWithDecreaseNumberOfMention(string phoneNumber)
        {
            var phoneDto = _context.FlatPhonesDto.First(x => x.Number == phoneNumber);
            phoneDto.NumberMentionsOnSite -= 1;
            _context.SaveChanges();
        }
    }
}
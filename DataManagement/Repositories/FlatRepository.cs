﻿using Application.Interfaces.Repository;
using Application.Models;
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

        public long ReadCountNotViewedFlats()
        {
            return _context.FlatDateInfosDto.Count(f => f.RefusePublication == null && f.TelegramPublication == null);
        }

        public FlatInfoClientModel ReadOldestNotViewedFlatInfoClientModel()
        {
            var noViewedFlatDateInfoDto = _context.FlatDateInfosDto
                .Where(d => d.RefusePublication == null && d.TelegramPublication == null)
                .OrderByDescending(x => x.SitePublication)
                .LastOrDefault();

            if (noViewedFlatDateInfoDto == null) return null;

            var flatModelDto = _context.FlatInfosDto.First(f => f.Id == noViewedFlatDateInfoDto.FlatInfoId);

            var flatPhoneModelDto = _context.FlatPhonesDto.First(p => p.Id == flatModelDto.FlatPhoneId);

            var flatInfoClientModel = new FlatInfoClientModel
            {
                Id = flatModelDto.Id,
                Title = flatModelDto.Title,
                Cost = flatModelDto.Cost,
                SitePublication = noViewedFlatDateInfoDto.SitePublication,
                Description = flatModelDto.Description,
                FlatPhoneClientModel = new FlatPhoneClientModel() { PhoneNumber = flatPhoneModelDto.Number, MentionOnSite = flatPhoneModelDto.NumberMentionsOnSite },
                LinksOfImages = ReadFlatImages(flatModelDto.Id),
                PageLink = flatModelDto.PageLink,
                ViewsOnSite = flatModelDto.ViewsOnSite.GetValueOrDefault(),
                FlatCoordinateClientModel = ReadFlatCoordinateOrGetDefault(flatModelDto.Id)
            };

            return flatInfoClientModel;
        }

        private FlatCoordinateClientModel ReadFlatCoordinateOrGetDefault(long flatId)
        {
            var flatCoordinateDto = _context.FlatCoordinatesDto.FirstOrDefault(c => c.FlatInfoId == flatId);

            if (flatCoordinateDto == null || flatCoordinateDto.Latitude == null || flatCoordinateDto.Longitude == null)
            {
                return new FlatCoordinateClientModel() { Latitude = 0, Longitude = 0 };
            }

            return new FlatCoordinateClientModel() { Latitude = flatCoordinateDto.Latitude.GetValueOrDefault(), Longitude = flatCoordinateDto.Longitude.GetValueOrDefault() };
        }

        private List<string> ReadFlatImages(long flatId)
        {
            return _context.FlatLinksImage.Where(l => l.FlatInfoId == flatId).Select(i => i.Link).ToList();
        }

        private void CreateFlat(FlatInfoModel flatInfoModel)
        {
            var phoneId = CreateOrUpgradePhoneNumberAndGetHisId(flatInfoModel.PhoneNumber);

            var flatInfoId = CreateFlatInfoAndGetHisId(flatInfoModel, phoneId);

            CreateFlatImages(flatInfoModel.LinksOfImages, flatInfoId);

            _context.FlatDateInfosDto.Add(new FlatDateInfoDto()
            { FlatInfoId = flatInfoId, SitePublication = flatInfoModel.SitePublication.ToUniversalTime() });
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
                FlatPhoneId = phoneId
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

        private long CreateOrUpgradePhoneNumberAndGetHisId(string number)
        {
            var phoneModel = _context.FlatPhonesDto.SingleOrDefault(p => p.Number == number);

            if (phoneModel == null)
            {
                var flatPhone = new FlatPhoneDto() { Number = number, NumberMentionsOnSite = 1 };
                _context.FlatPhonesDto.Add(flatPhone);
                _context.SaveChanges();
                return flatPhone.Id;
            }

            phoneModel.NumberMentionsOnSite += 1;
            _context.FlatPhonesDto.Update(phoneModel);
            _context.SaveChanges();

            return phoneModel.Id;
        }
    }
}

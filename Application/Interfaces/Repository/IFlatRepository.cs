﻿using WebScraper.Models;

namespace Application.Interfaces.Repository
{
    public interface IFlatRepository
    {
        void CreateFlats(List<FlatInfoModel> flats);

        long ReadCountNotViewedFlats();

        FlatInfoModel ReadNotViewedFlatInfoModel();
    }
}

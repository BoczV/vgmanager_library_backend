using AutoMapper;
using VGManager.Adapter.Models.Requests;
using VGManager.Adapter.Models.Requests.VG;
using VGManager.Library.Api.Common;
using VGManager.Library.Api.Endpoints.Project.Response;

namespace VGManager.Library.Api.MapperProfiles;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<ProjectRequest, ProjectResponse>()
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Project.Name));
        CreateMap<BasicRequest, BaseModel>();
    }
}

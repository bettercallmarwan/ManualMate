//using ManualMate.Application.Interfaces;
//using ManualMate.Application.Interfaces.Repositories;
//using ManualMate.Application.Interfaces.Services;
//using StackExchange.Redis;

//namespace ManualMate.Application.Services
//{
//    public class ServiceManager : IServiceManager
//    {
//        private readonly IConnectionMultiplexer _multiplexer;
//        private readonly IHttpClientFactory _clientFactory;
//        private readonly IItemRepository _itemRepository;


//        private readonly Lazy<ICacheService> _cacheService;
//        private readonly Lazy<IEmbeddingService> _embeddingService;
//        private readonly Lazy<ILlmService> _llmService;
//        private readonly Lazy<IFileProcessingService> _fileProcessingService;
//        private readonly Lazy<IQaService> _manualQaService;
//        private readonly Lazy<FileUploadService> _fileUploadService;

//        public ServiceManager(IConnectionMultiplexer multiplexer, IHttpClientFactory clientFactory)
//        {
//            _multiplexer = multiplexer;
//            _clientFactory = clientFactory;

//            _cacheService = new Lazy<ICacheService>(() => new RedisCacheService(_multiplexer));
//            _embeddingService = new Lazy<IEmbeddingService>(() => new HuggingFaceEmbeddingService(_clientFactory));
//            _llmService = new Lazy<ILlmService>(() => new GeminiLlmService(_clientFactory));
//            _fileProcessingService = new Lazy<IFileProcessingService>(() => new FileProcessingService());
//        }

//        public ICacheService CacheService => _cacheService.Value;

//        public IEmbeddingService EmbeddingService => _embeddingService.Value;

//        public ILlmService LlmService => _llmService.Value;

//        public IFileProcessingService ManualProcessingService => throw new NotImplementedException();

//        public IManualQaService ManualQaService => throw new NotImplementedException();

//        public FileUploadService FileUploadService => throw new NotImplementedException();
//    }
//}

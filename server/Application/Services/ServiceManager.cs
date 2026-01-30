//using ManualMate.Application.Interfaces;
//using ManualMate.Infrastructure.Repositories;
//using StackExchange.Redis;

//namespace ManualMate.Application.Services
//{
//    public class ServiceManager : IServiceManager
//    {
//        private readonly IConnectionMultiplexer _multiplexer;
//        private readonly IHttpClientFactory _clientFactory;
//        private readonly ProductRepository _productRepository;


//        private readonly Lazy<ICacheService> _cacheService;
//        private readonly Lazy<IEmbeddingService> _embeddingService;
//        private readonly Lazy<ILlmService> _llmService;
//        private readonly Lazy<IManualProcessingService> _manualProcessingService;
//        private readonly Lazy<IManualQaService> _manualQaService;
//        private readonly Lazy<FileUploadService> _fileUploadService;

//        public ServiceManager(IConnectionMultiplexer multiplexer, IHttpClientFactory clientFactory)
//        {
//            _multiplexer = multiplexer;
//            _clientFactory = clientFactory;

//            _cacheService = new Lazy<ICacheService>(() => new RedisCacheService(_multiplexer));
//            _embeddingService = new Lazy<IEmbeddingService>(() => new HuggingFaceEmbeddingService(_clientFactory));
//            _llmService = new Lazy<ILlmService>(() => new GeminiLlmService(_clientFactory));
//            _manualProcessingService = new Lazy<IManualProcessingService>(() => new ManualProcessingService());
//        }

//        public ICacheService CacheService => _cacheService.Value;

//        public IEmbeddingService EmbeddingService => _embeddingService.Value;

//        public ILlmService LlmService => _llmService.Value;

//        public IManualProcessingService ManualProcessingService => throw new NotImplementedException();

//        public IManualQaService ManualQaService => throw new NotImplementedException();

//        public FileUploadService FileUploadService => throw new NotImplementedException();
//    }
//}

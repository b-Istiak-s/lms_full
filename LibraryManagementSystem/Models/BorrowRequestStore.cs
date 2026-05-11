namespace LibraryManagementSystem.Models
{
    // Shared in-memory store for member borrow requests during prototype/demo.
    public class BorrowRequestStore
    {
        private readonly List<BorrowRequest> _requests = new();
        private int _nextId = 1;

        public List<BorrowRequest> Requests => _requests;

        public BorrowRequest Add(BorrowRequest request)
        {
            request.RequestId = _nextId++;
            _requests.Add(request);
            return request;
        }

        public BorrowRequest? Find(int requestId)
        {
            return _requests.FirstOrDefault(r => r.RequestId == requestId);
        }
    }
}

using PetStoreClient;
using System.ComponentModel;
using System.Diagnostics;

namespace PetStoreTest
{
    public class APITest
    {
        private const string baseUrl = "https://petstore.swagger.io/v2";
        private const string apiKey = "special-key";

        private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        private static readonly PetStoreRestClient petClient = new PetStoreRestClient(httpClient);

        [Fact]
        public async void GetPetById_WhenPetExists_ReturnsPet()
        {
            var setupPet = new Pet()
            {
                Id = await GetUnusedId(),
                Name = "Rex",
                Status = PetStatus.Available
            };

            await petClient.AddPetAsync(setupPet);
            var fetchedPet = await petClient.GetPetByIdAsync((long)setupPet.Id);
            Assert.NotNull(fetchedPet);
            Assert.Equal(setupPet.Id, fetchedPet.Id);
            Assert.Equal(setupPet.Name, fetchedPet.Name);
            Assert.Equal(setupPet.Status, fetchedPet.Status);

            await petClient.DeletePetAsync(apiKey, (long)setupPet.Id);
        }

        [Fact]
        public async void GetPetById_WhenPetDoesNotExist_Returns404()
        {
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
                await petClient.GetPetByIdAsync(await GetUnusedId()));
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async void PostPet_WhenPetAlreadyExists_Returns405()
        {
            var setupPet = new Pet()
            {
                Id = await GetUnusedId(),
                Name = "Filo",
                Status = PetStatus.Available
            };
            await petClient.AddPetAsync(setupPet);

            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
                await petClient.AddPetAsync(setupPet));
            Assert.Equal(405, exception.StatusCode);

            await petClient.DeletePetAsync(apiKey, (long)setupPet.Id);
        }

        [Fact]
        public async void PostPet_WhenPetValid_AddsPet()
        {
            var pet = new Pet()
            {
                Id = await GetUnusedId(),
                Name = "Filo",
                Status = PetStatus.Available
            };
            await petClient.AddPetAsync(pet);
            
            var petResponse = await petClient.GetPetByIdAsync((long)pet.Id);
            Assert.NotNull(petResponse);
            Assert.Equal(pet.Id, petResponse.Id);
            Assert.Equal(pet.Name, petResponse.Name);
            Assert.Equal(pet.Status, petResponse.Status);
            
            await petClient.DeletePetAsync(apiKey, (long)pet.Id);
        }

        [Fact]
        public async void PutPet_WhenPetExists_UpdatesPet()
        {
            var setupPet = new Pet()
            {
                Id = await GetUnusedId(),
                Name = "Harry",
                Status = PetStatus.Available
            };
            await petClient.AddPetAsync(setupPet);
            
            var updatedPet = new Pet()
            {
                Id = setupPet.Id,
                Name = "Harry",
                Status = PetStatus.Pending
            };

            await petClient.UpdatePetAsync(updatedPet);
            var petResponse = await petClient.GetPetByIdAsync((long)updatedPet.Id);
            Assert.NotNull(petResponse);
            Assert.Equal(updatedPet.Id, petResponse.Id);
            Assert.Equal(updatedPet.Name, petResponse.Name);
            Assert.Equal(updatedPet.Status, petResponse.Status);
            await petClient.DeletePetAsync(apiKey, (long)updatedPet.Id);
        }

        [Fact]
        public async void PutPet_WhenPetDoesNotExist_Returns404()
        {
            var pet = new Pet()
            {
                Id = await GetUnusedId(),
                Name = "Filo",
                Status = PetStatus.Available
            };
            var exception = await Assert.ThrowsAsync<ApiException>(async () => await petClient.UpdatePetAsync(pet));
            Assert.Equal(404, exception.StatusCode);

            await petClient.DeletePetAsync(apiKey, (long)pet.Id);
        }

        [Fact]
        public async void DeletePet_WhenPetExists_DeletesPet()
        {
            var setupPet = new Pet()
            {
                Id = await GetUnusedId(),
                Name = "Harry",
                Status = PetStatus.Available
            };
            await petClient.AddPetAsync(setupPet);
            await petClient.DeletePetAsync(apiKey, (long)setupPet.Id);
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
                await petClient.GetPetByIdAsync((long)setupPet.Id));
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async void DeletePet_WhenPetDoesNotExist_Returns404()
        {
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
                await petClient.DeletePetAsync(apiKey, await GetUnusedId()));
            Assert.Equal(404, exception.StatusCode);
        }

        private static async Task<long> GetUnusedId()
        {
            var random = new Random(88572);

            for (int attempts = 0; attempts < 100; attempts++)
            {
                var id = random.Next(100000000, 100999999);
                try
                {
                    await petClient.GetPetByIdAsync(id);
                }
                catch (ApiException e)
                {
                    if (e.StatusCode == 404)
                    {
                        return id;
                    }
                }
            }
            throw new Exception("Could not find an unused ID after 100 attempts");
        }
    }
}

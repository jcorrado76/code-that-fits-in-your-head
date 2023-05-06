from fastapi.testclient import TestClient
from hello_world import app

client = TestClient(app)

def test_get_hello_world():
    response = client.get('/')
    assert response.status_code == 200
    assert response.json() == {"message": "Hello World"}
import { toast } from "react-toastify";
import { useGetGameMutation } from "../slices/gamesApiSlice";
import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Container } from "react-bootstrap";

function GamePage() {
  const [getGame] = useGetGameMutation();
  const { gameId } = useParams();

  const fetchGameData = async (id: string = ''): Promise<void> => {
    try {
        // dispatch the query via redux
        const response = await getGame(id).unwrap();
        return response;
      } catch (error: any) {
        toast.error('Failed to fetch game data.');
        console.error(error);
      }
  };

  const { data, isLoading, error } = useQuery({
    queryKey: ['game', gameId],
    queryFn: () => fetchGameData(gameId)
  })

  console.log(data);

  return (
    <>
      <Container>
        GamePage: {gameId}
      </Container>
    </>
  )
}

export default GamePage;

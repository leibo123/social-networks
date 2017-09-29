curTime = cotalkdata317{1, 2};
%filename = 'testAnimated.gif'; % Specify the output file name
%im = cell(height(cotalkdata317), 1);
vobj=VideoWriter('MyMovieFile', 'Motion JPEG AVI');
vobj.FrameRate=40;
vobj.Quality=100;
open(vobj);

for i = 900000:height(cotalkdata317)
    id = cotalkdata317{i, 1};
        if strcmp(id, "T2B") == 1 || strcmp(id, "T1B") == 1
            color = [0 0.5 0];
        elseif strcmp(id, "4B") == 1 || strcmp(id, "8B") == 1 || strcmp(id, "9B") == 1
            color = 'r';
        else
            color = 'b';
        end
    if strcmp(curTime, cotalkdata317{i, 2}) ~= 1
        %{
        frame = getframe(gcf);
        im{i} = frame2im(frame);
        [A,map] = rgb2ind(im{i},256);
        if i == 1000000
            imwrite(A, map, filename,'gif','LoopCount',Inf,'DelayTime',0.1);
        else
            imwrite(A, map, filename,'gif','WriteMode','append','DelayTime',0.1);
        end
        %}
        F=getframe(gcf);
        writeVideo(vobj, F);
        cla(gca)
        hold off
        axis([0 9.01 0 6.01])
        pause(0);
        curTime = cotalkdata317{i, 2};
    end
    axis([0 9.01 0 6.01])
    if cotalkdata317{i, 5} == "True"
        if strcmp(id, "Lab1B") == 0 && strcmp(id, "Lab2B") == 0 && strcmp(id, "Lab3B") == 0
        plot(cotalkdata317{i,3}, cotalkdata317{i, 4}, 'o', 'MarkerSize', 16, 'MarkerFaceColor', color, 'MarkerEdgeColor', 'black', 'LineWidth', 4.0)
        end
    else
        if strcmp(id, "Lab1B") == 0 && strcmp(id, "Lab2B") == 0 && strcmp(id, "Lab3B") == 0
        plot(cotalkdata317{i,3}, cotalkdata317{i, 4}, 'o', 'MarkerSize', 16, 'MarkerFaceColor', color)
        end
    end
    text(cotalkdata317{i, 3}-0.17, cotalkdata317{i, 4}, char(cotalkdata317{i,1}), 'Color', 'w')
    title(char(cotalkdata317{i, 2}))
    a=findobj(gcf);
    alltext=findall(a,'Type','text');
    set(alltext,'FontSize',8);
    hold on
end
close(vobj)
            
            
            
            
        